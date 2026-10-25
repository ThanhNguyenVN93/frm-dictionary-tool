# Dictionary Tool — EN / VI / ZH

Tra từ điển **Anh · Việt · Trung** trong một cửa sổ duy nhất.  
Ứng dụng desktop WinForms (.NET Framework 4.8), không cần cài đặt trình duyệt hay kết nối liên tục.

---

## Tính năng

| Tính năng | Mô tả |
|-----------|-------|
| **3 cột song song** | EN, VI, ZH hiển thị cùng lúc sau một lần tìm kiếm |
| **Nhập bất kỳ ngôn ngữ** | Tự nhận diện EN / VI / ZH, hoặc chốt tay bằng chip góc trái |
| **Tìm kiếm bằng Enter** | Nhấn Enter để tra — không tra liên tục khi đang gõ |
| **Tự động tra (tùy chọn)** | Bật trong menu ℹ️, chọn delay 0.5s–3s |
| **Câu ví dụ** | Tatoeba EN↔VI, EN↔ZH |
| **Phát âm (TTS)** | Nút 🔊 cho EN (audio gốc) + VI/ZH (Google TTS), có cache |
| **Đồng / trái nghĩa** | Hiển thị synonyms / antonyms cho từ tiếng Anh |
| **Tách chữ Hán** | Từ ghép ZH → pinyin + nghĩa của từng ký tự (từ CC-CEDICT) |
| **Nhãn nguồn** | Badge cho biết kết quả từ online hay offline + tên nguồn |
| **Ôn tập giãn cách** | SRS kiểu Anki cho từ đã lưu (Quên/Khó/Nhớ/Dễ), menu ℹ️ |
| **Cỡ chữ** | Phóng to/thu nhỏ nội dung (Nhỏ → Rất lớn), menu ℹ️ |
| **Giao diện sáng/tối** | Toggle Light/Dark theme trong menu ℹ️ |
| **Clipboard Watch** | Tự động tra khi copy text từ ứng dụng khác |
| **Global hotkey** | Mặc định `Ctrl+Shift+D`, tuỳ chỉnh qua dialog ⚙ |
| **Mini mode** | Thu gọn cửa sổ để xem cạnh tài liệu |
| **Đánh dấu & Lịch sử** | Lưu từ yêu thích, click để tra lại ngay (không gọi API); có ô **lọc nhanh** theo từ |
| **Sao lưu / phục hồi** | Xuất/nhập toàn bộ bookmark **kèm tiến độ ôn tập (SRS)** ra file `.json` để backup / đổi máy |
| **Cửa sổ Cài đặt** | Gom theme, cỡ chữ, tự động tra, clipboard, chạy nền ở khay, phím tắt về một chỗ (menu ℹ️ → Cài đặt) |
| **Chạy nền ở khay** | Icon system tray; bật "chạy nền" thì đóng/thu nhỏ = ẩn xuống khay (hotkey vẫn sống), double-click để mở lại |
| **Góp ý bản dịch** | Lưu phản hồi local → xuất `.xlsx` hoặc Google Sheets |
| **Giao diện VI/EN** | Toggle ngôn ngữ giao diện trong menu ℹ️ |
| **Lưu trạng thái** | Nhớ ngôn ngữ, clipboard, chip lock, auto-search, theme, cỡ chữ, chạy-nền qua lần khởi động |

---

## Nguồn dữ liệu

### Tiếng Anh (EN)
| Nguồn | Loại | Ghi chú |
|-------|------|---------|
| dictionaryapi.dev | Online, miễn phí, không key | IPA, definitions, examples |
| Merriam-Webster Collegiate | Online, cần API key, 1000/ngày | Short definitions, ngắn gọn |
| Merriam-Webster Learner's | Online, cần API key, 1000/ngày | Fallback nếu Collegiate rỗng |

### Tiếng Việt (VI → EN)
| Nguồn | Loại | Thứ tự fallback |
|-------|------|-----------------|
| OVDP StarDict (vi-en) | Offline, SQLite | Bước 1: tra nguyên văn |
| EVDict StarDict (vi-en-ev) | Offline, SQLite | Bước 1: tra nguyên văn (ưu tiên cao hơn OVDP) |
| — (bỏ loại từ đứng đầu) | — | Bước 2: "con mèo" → "mèo" |
| — (bỏ dấu, ASCII only) | — | Bước 3: "khong" → "không" |
| — (bỏ loại từ + bỏ dấu) | — | Bước 4 |
| Wiktionary API | Online, cache 30 ngày | Bước 5: tra nguyên cụm từ |
| Google Translate (unofficial) | Online, không key | Bước 6: fallback cuối |

### Tiếng Trung (ZH)
| Nguồn | Loại |
|-------|------|
| CC-CEDICT | Offline, SQLite (~120k entries) |

### Câu ví dụ
| Nguồn | Loại |
|-------|------|
| Tatoeba (EN↔VI, EN↔ZH) | Offline, SQLite |

---

## Yêu cầu hệ thống

- Windows 10 / 11
- [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48) (thường đã có sẵn)

---

## Cài đặt & chạy từ source

### 1. Clone repo

```bash
git clone https://github.com/ThanhNguyenVN93/frm-dictionary-tool.git
cd frm-dictionary-tool
```

### 2. Cấu hình API key Merriam-Webster

```bash
cp DictionaryTool.UI/App.config.example DictionaryTool.UI/App.config
```

Mở `DictionaryTool.UI/App.config` và điền key vào:

```xml
<add key="MerriamWebster:CollegiateApiKey" value="KEY_CỦA_BẠN" />
<add key="MerriamWebster:LearnerApiKey"    value="KEY_CỦA_BẠN" />
```

> Đăng ký miễn phí tại [dictionaryapi.com](https://dictionaryapi.com) — 1 000 query/ngày/key.  
> Cột VI và ZH vẫn hoạt động ngay cả khi chưa có key.

### 3. Build & chạy

Mở `DictionaryTool.sln` bằng Visual Studio 2022+ và nhấn **F5**, hoặc build từ command line:

```
"C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\MSBuild.exe" DictionaryTool.sln /p:Configuration=Release /t:Restore,Build
```

> App xuất ra **1 file `Dictionary Tool.exe`** — toàn bộ DLL managed đã nhúng vào exe
> bằng **Costura.Fody**. Chỉ còn `Data/` (từ điển offline) + `x86/`,`x64/SQLite.Interop.dll`
> (engine SQLite native) cần đi kèm cạnh exe.

### 4. Đóng gói phát hành (installer)

Thư mục `installer/` chứa sẵn bộ đóng gói:

```
pwsh installer\build-release.ps1
```

Lệnh này build Release rồi dựng `installer\release\Dictionary Tool\` (exe + Data + native
+ 2 file `.bat`). Copy nguyên thư mục đó đi phát; người dùng chọn:

- **`installer.bat`** — cài vào `%LOCALAPPDATA%\Dictionary Tool` (không cần Admin), tạo
  shortcut Desktop + Start Menu, kèm `uninstall.bat`.
- **`portable.bat`** — tạo bản portable chạy liền, không đụng registry/shortcut hệ thống;
  cấu hình lưu cạnh exe nên mang qua USB/máy khác vẫn giữ nguyên.

Muốn 1 file `setup.exe` chuyên nghiệp (có Add/Remove Programs): cài **Inno Setup** rồi build
`installer\DictionaryTool.iss`.

---

## Cấu trúc project

```
DictionaryTool.sln
├── DictionaryTool.Core/              — Logic thuần (models, repositories, services), không phụ thuộc WinForms
│   ├── Models/                       — TrilingualResult, WordLookupResult, StardictEntry, ...
│   ├── Repositories/
│   │   ├── StardictRepository        — VI↔EN offline (OVDP + EVDict, SQLite, WAL mode)
│   │   ├── CcCedictRepository        — ZH offline (CC-CEDICT, SQLite, WAL mode)
│   │   ├── WiktionaryViEnRepository  — VI→EN online, cache 30 ngày trong trilingual.db
│   │   ├── TatoebaRepository         — Câu ví dụ offline
│   │   ├── CacheRepository           — EN word cache (cache.db)
│   │   ├── HistoryRepository         — Lịch sử tra + TrilingualCache
│   │   ├── BookmarkRepository        — Bookmark
│   │   └── UserFeedbackRepository    — Góp ý bản dịch (UserFeedback table)
│   ├── Clients/
│   │   ├── DictionaryApiDevClient    — dictionaryapi.dev
│   │   ├── MerriamWebsterClient      — MW Collegiate + Learner's
│   │   └── GoogleTranslateClient     — Unofficial GT, không key
│   ├── TrilingualLookupService       — Điều phối toàn bộ fallback chain
│   ├── DictionaryService             — EN lookup + cache
│   ├── LanguageDetector              — Tự nhận diện EN/VI/ZH
│   └── VietnameseNormalizer          — Bỏ dấu, bỏ loại từ, chuẩn hóa
│
├── DictionaryTool.UI/                — WinForms app (Guna.UI2 dark theme)
│   ├── MainForm.cs / .Designer.cs    — Cửa sổ chính, 3 cột
│   ├── FeedbackDialog.cs             — Dialog góp ý bản dịch (2 tab: nhập mới + danh sách)
│   ├── HistoryBookmarkForm.cs        — Dialog lịch sử & bookmark
│   ├── HotkeyConfigDialog.cs         — Cấu hình global hotkey
│   ├── DonateForm.cs                 — Dialog ủng hộ (Momo QR)
│   ├── HelpForm.cs                   — Hướng dẫn sử dụng
│   ├── UiStrings.cs                  — Toàn bộ chuỗi UI (VI/EN toggle)
│   ├── UiPalette.cs                  — Màu sắc dark theme
│   ├── Program.cs                    — Entry point, khởi tạo services + DI thủ công
│   └── Data/
│       ├── stardict/vi-en/           — OVDP StarDict VI→EN (.idx/.dict.dz)
│       ├── stardict/en-vi/           — StarDict EN→VI (.idx/.dict.dz)
│       ├── stardict/vi-en-ev/        — EVDict StarDict VI→EN (.idx/.dict.dz)
│       ├── cedict_ts.u8 (hoặc .gz)   — CC-CEDICT
│       └── tatoeba/                  — Câu ví dụ (tsv)
│
└── DictionaryTool.Tests/             — MSTest, 86 tests, fakes thủ công (không Moq)
```

---

## Databases (runtime)

```
%AppData%\DictionaryTool\
├── cache.db        — WordCache (EN word → WordLookupResult JSON, zero TTL)
└── trilingual.db   — SearchHistory, Bookmarks, TrilingualCache,
                       StardictViEn, StardictEnVi, CcCedictEntries, TatoebaExamples,
                       ImportMeta, WiktionaryViEnCache, UserFeedback
```

---

## Luồng tra từ

```
User input → LanguageDetector.Detect(input)

  EN → DictionaryService (dictionaryapi.dev + MW Collegiate → Learner's fallback)
       → StardictRepository.GetViAsync(word)            → VI result
       → CcCedictRepository.GetAsync(word)              → ZH result

  VI → VietnameseNormalizer fallback chain (6 bước):
         1. OVDP + EVDict exact match
         2. Bỏ loại từ đầu (con mèo → mèo)
         3. Bỏ dấu ASCII-only (khong → không)
         4. Bỏ loại từ + bỏ dấu
         5. Wiktionary API (nguyên cụm, cache 30 ngày)
         6. Google Translate unofficial (nguyên cụm)
       → bridge EN word → rồi như luồng EN ở trên

  ZH → CcCedictRepository (field "translations" lấy bridge EN word)
       → rồi như luồng EN ở trên
```

---

## Quy ước dev

### SQLite
- Raw ADO.NET (`System.Data.SQLite`), không ORM
- Mỗi repository tự tạo table trong constructor (`CREATE TABLE IF NOT EXISTS`)
- Tất cả connection đều bật `PRAGMA journal_mode=WAL; PRAGMA busy_timeout=5000;`
- Thêm cột mới: `ALTER TABLE ... ADD COLUMN` trong `try/catch` (bỏ qua nếu cột đã tồn tại)
- `ALTER TABLE` phải chạy **trước** `CREATE INDEX` trên cột mới

### Test
- MSTest, fakes thủ công implement interface (không Moq/NSubstitute)
- Mỗi test tạo file SQLite temp riêng (`Path.GetTempPath() + Guid + ".db"`), cleanup trong `[TestCleanup]`

### Build
- **Bắt buộc dùng MSBuild.exe của Visual Studio** (old-style csproj, net48) — không dùng `dotnet build`
- Khi thêm file `.cs` mới vào `DictionaryTool.UI`: phải thêm `<Compile Include="..." />` vào `.csproj`
- `DictionaryTool.Core` dùng wildcard `**\*.cs` — không cần thêm tay

### WinForms
- `MainForm` có `.Designer.cs` riêng — constructor không tham số, không biến cục bộ trong `InitializeComponent`
- Form phụ (`FeedbackDialog`, `HistoryBookmarkForm`, v.v.) viết 1 file, không qua Designer
- Layout Full/Mini mode dùng cặp hằng `FullXxx`/`MiniXxx` trong `ApplyWindowMode()`

---

## Cảnh báo quan trọng

1. **API keys**: `App.config` chứa MW API key thật — **không commit**.
2. **Data files**: StarDict `.dict.dz`, CC-CEDICT, Tatoeba là file lớn, thường nằm trong `.gitignore` — cần copy thủ công vào `Data/` trước khi build.
3. **Build CI**: chạy `MSBuild -t:Restore` trước `Build` nếu dùng CLI (cần `<RuntimeIdentifiers>win</RuntimeIdentifiers>` trong Core.csproj).

---

## Báo lỗi / Góp ý

Dùng tính năng **Góp ý bản dịch** (menu ℹ️ trong app) để lưu phản hồi local và xuất ra `.xlsx`, hoặc mở [issue trên GitHub](https://github.com/ThanhNguyenVN93/frm-dictionary-tool/issues).

---

## Ủng hộ tác giả

Nếu thấy hữu ích, bạn có thể ủng hộ qua **Ko-fi** hoặc quét mã **Momo** trong app (nút 🤝 menu ℹ️).

---

## License

[MIT](LICENSE)
