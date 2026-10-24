# Dictionary Tool — Project Context

Ứng dụng desktop Windows (C#, WinForms, .NET Framework 4.8) tra từ điển 3 ngôn ngữ EN/VI/ZH song song. Nhập bất kỳ ngôn ngữ nào → hiển thị kết quả cả 3 cùng lúc, nối qua tiếng Anh làm cầu.

---

## Cấu trúc solution

```
DictionaryTool.sln
├── DictionaryTool.Core/   — toàn bộ logic (models, repositories, services), KHÔNG phụ thuộc WinForms
├── DictionaryTool.UI/     — WinForms + Guna.UI2.WinForms, toàn bộ UI dựng bằng code (không Designer kéo-thả)
└── DictionaryTool.Tests/  — MSTest, 86 tests (tất cả pass)
```

**Build**: dùng MSBuild.exe của Visual Studio (old-style csproj target net48):
```
"C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\MSBuild.exe" DictionaryTool.sln /p:Configuration=Debug /v:q
```

**Chạy test**: dùng vstest.console.exe (không dùng `dotnet test` vì csproj format cũ):
```
"C:\Program Files\Microsoft Visual Studio\18\Professional\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" DictionaryTool.Tests\bin\Debug\DictionaryTool.Tests.dll /logger:console
```

---

## Trạng thái: 86/86 test pass, build sạch

### Đã hoàn thành

**Module gốc (EN-only):**
- Tra từ tiếng Anh qua `dictionaryapi.dev` (miễn phí, không key) + Merriam-Webster Collegiate/Learner's (cần key, 1000/ngày)
- Cache SQLite (`cache.db`, bảng `WordCache`) — zero TTL (định nghĩa gần như bất biến)
- MW optimization: gọi Collegiate trước, chỉ gọi Learner's nếu Collegiate trả rỗng (~50% quota tiết kiệm)
- UI 1 cột: IPA, icon loa 🔊, danh sách nghĩa, Guna2 dark theme

**Phase 1 — Mở rộng 3 ngôn ngữ:**
- Bundle offline: CC-CEDICT (ZH, ~120k entries), Stardict EN-VI + VI-EN (OVDP, ~60k entries), Tatoeba câu ví dụ — tất cả trong `DictionaryTool.UI/Data/`, import vào SQLite lần đầu khởi động
- `TrilingualLookupService`: điều phối EN/VI/ZH, bắc cầu qua EN (VI → EN bridge word → ZH; ZH → pinyin → EN → VI)
- UI 3 cột: EN (MW + definitions), VI (Stardict), ZH (CC-CEDICT + pinyin)
- Model `TrilingualResult` gộp 3 kết quả

**Phase 2:**
- Clipboard watcher: toggle, tự tra khi copy text (Win32 `AddClipboardFormatListener`)
- Global hotkey: Ctrl+Shift+D mặc định, cấu hình được qua dialog ⚙ (`HotkeyConfigDialog`), lưu vào `App.config`
- Mini mode: thu nhỏ cửa sổ, 3 cột thu nhỏ (không ẩn cột nào)

**Phase 3:**
- Bookmark (nút ★/☆), lịch sử tra (`SearchHistory` table trong `trilingual.db`)
- `HistoryBookmarkForm`: dialog xem lịch sử + bookmark, tra lại 1 click, xoá bookmark
- Export bookmark ra `.xlsx` (EPPlus)

**Phase 4:**
- Gõ tiếng Việt không dấu: "khong" → nhận đúng VI, tra ra "không" (`VietnameseNormalizer` + index `WordNoDiacritics` trong `StardictViEn`)
- Tự động bỏ loại từ đứng đầu: "con mèo" → "mèo" (`VietnameseNormalizer.StripLeadingClassifier`)
- Autocomplete gợi ý cả EN + VI + ZH đã tra (`CacheRepository.GetAllCachedViWordsAsync/ZhWordsAsync` query `SearchHistory`)
- Hotkey cấu hình được: nút ⚙ cạnh mini mode, dialog `HotkeyConfigDialog`, đọc/ghi `App.config`
- Click lịch sử hiện kết quả cached ngay (không gọi lại API): `TrilingualCache` table trong `trilingual.db`, `HistoryRepository.SaveTrilingualResultAsync/GetTrilingualResultAsync`, `SearchFromHistoryAsync` trong `MainForm`

---

## Kiến trúc key

### Databases

```
%AppData%\DictionaryTool\
├── cache.db       — WordCache (EN word → WordLookupResult JSON)
└── trilingual.db  — SearchHistory, Bookmarks, TrilingualCache, StardictViEn, StardictEnVi,
                     CcCedictEntries, TatoebaExamples
```

### Services

```
TrilingualLookupService (ITrilingualLookupService)
├── DictionaryService (IDictionaryService)         — EN lookup via dictionaryapi.dev + MW
│   ├── DictionaryApiDevClient
│   ├── MerriamWebsterClient                       — Collegiate-first, Learner's fallback
│   └── CacheRepository (ICacheRepository)         — WordCache in cache.db
├── CcCedictRepository (ICcCedictRepository)       — ZH lookup
├── StardictRepository (IStardictRepository)       — VI lookup (EN-VI, VI-EN)
├── TatoebaRepository (ITatoebaRepository)          — câu ví dụ
├── HistoryRepository (IHistoryRepository)          — SearchHistory + TrilingualCache
└── BookmarkRepository (IBookmarkRepository)        — Bookmarks
```

### Luồng tra từ

```
User input → LanguageDetector.Detect(input)
  EN → DictionaryService.LookupAsync → EN result
       → StardictRepository.GetViAsync(englishWord) → VI result
       → CcCedictRepository.GetAsync(englishWord)  → ZH result

  VI → VietnameseNormalizer chain (4 bước: nguyên văn → bỏ loại từ → bỏ dấu → cả 2)
       → StardictRepository.GetEnAsync(viWord) → bridge EN word
       → rồi như luồng EN ở trên

  ZH → CcCedictRepository.GetAsync(zhWord) → bridge EN word (từ field "translations")
       → rồi như luồng EN ở trên
```

### MW API optimization

`DictionaryService.SafeGetMerriamWebsterAsync`:
- Gọi `GetCollegiateAsync` trước
- Nếu Collegiate trả ≥1 meaning → dùng luôn, **bỏ qua** `GetLearnerAsync` (tiết kiệm ~50% quota)
- Nếu Collegiate rỗng → gọi `GetLearnerAsync` làm fallback

---

## Quy ước code (bắt buộc giữ nguyên)

### SQLite
- Raw ADO.NET (`System.Data.SQLite`), không ORM
- `CREATE TABLE IF NOT EXISTS` trong constructor của mỗi repository
- Thêm cột vào bảng cũ phải dùng `ALTER TABLE ... ADD COLUMN` bọc `try/catch` (SQLiteException nếu cột đã tồn tại — bỏ qua)
- **Thứ tự bắt buộc**: `ALTER TABLE` PHẢI chạy TRƯỚC `CREATE INDEX` trên cột mới (đã có bug thật khi làm ngược)

### Test
- MSTest, fake thủ công implement interface trực tiếp (không Moq/NSubstitute)
- Mỗi test tự tạo file SQLite temp riêng (`Path.GetTempPath() + Guid + ".db"`), cleanup trong `[TestCleanup]`
- `FakeDictionaryApiDevClient`, `FakeMerriamWebsterClient`, `FakeCacheRepository`, `FakeHistoryRepository` trong `DictionaryTool.Tests/Fakes/`

### WinForms
- `MainForm` có file `.Designer.cs` riêng (bắt buộc: constructor không tham số, class đầu tiên trong file, không biến cục bộ trong `InitializeComponent`)
- Form phụ (`HistoryBookmarkForm`, `HotkeyConfigDialog`) viết 1 file, không qua Designer, dựng control trong constructor
- Layout Full/Mini mode dùng cặp hằng `FullXxx`/`MiniXxx` trong `ApplyWindowMode()`

### Đặt tên
- Repository → `DictionaryTool.Core/Repositories/`
- Client → `DictionaryTool.Core/Clients/`
- Service → `DictionaryTool.Core/`
- Model → `DictionaryTool.Core/Models/`
- Normalizer/helper → `DictionaryTool.Core/` (e.g. `VietnameseNormalizer.cs`)

---

## Cảnh báo quan trọng

1. **API keys**: `DictionaryTool.UI/App.config` chứa MW API key thật (plaintext) — **không commit, không share ra ngoài**.
2. **Dữ liệu thật vs. fixture**: 2 bug nghiêm trọng nhất (CC-CEDICT chọn nhầm nghĩa, ALTER TABLE/CREATE INDEX sai thứ tự) chỉ lộ ra khi test với DB thật. Unit test với fixture rỗng không đủ.
3. **Chụp màn hình UI**: app không thể test qua computer-use trực tiếp. Dùng `System.Windows.Automation` (PowerShell) để tương tác UI nếu cần — cách này đã dùng thành công ở Phase 3. Lưu ý: `HistoryBookmarkForm` và `SaveFileDialog` xuất hiện là con của cửa sổ chính trong cây UI Automation.
4. **Build**: phải dùng MSBuild.exe của Visual Studio, không dùng `dotnet build` (csproj format cũ, net48).

---

## Còn lại / có thể làm tiếp

- **Module 3 (YouGlish)**: placeholder panel đã có trong UI, chờ điều khoản sử dụng từ YouGlish.
- Test với dữ liệu DB thật (CC-CEDICT, Stardict) để kiểm tra corner case thực tế.
- Phát âm audio (icon 🔊 đã có trong UI, cần download + cache mp3 và phát qua NAudio).
