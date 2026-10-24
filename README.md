# 📖 Dictionary Tool — EN / VI / ZH

Tra từ điển **Anh · Việt · Trung** trong một cửa sổ duy nhất.  
Ứng dụng desktop WinForms, không cần cài đặt trình duyệt hay kết nối liên tục.

---

## ✨ Tính năng

| Tính năng | Mô tả |
|-----------|-------|
| **3 cột song song** | EN, VI, ZH hiển thị cùng lúc sau một lần tìm kiếm |
| **Phát âm** | Phát âm IPA từ dictionaryapi.dev (click 🔊) |
| **Câu ví dụ** | Tatoeba EN↔VI, EN↔ZH |
| **Clipboard Watch** | Tự động tra từ vừa copy từ ứng dụng khác |
| **Hotkey toàn cục** | Mặc định `Ctrl+Shift+D`, tuỳ chỉnh được |
| **Mini mode** | Thu gọn cửa sổ để xem cạnh tài liệu |
| **Đánh dấu & Lịch sử** | Lưu từ yêu thích, xem lại lịch sử tra |
| **Giao diện VI/EN** | Toggle ngôn ngữ giao diện trong menu ℹ️ |
| **Lưu trạng thái** | Nhớ ngôn ngữ, clipboard, chip lock qua lần khởi động |

---

## 🖥️ Yêu cầu hệ thống

- Windows 10/11
- [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48) (thường đã có sẵn)

---

## 🚀 Cài đặt & chạy từ source

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

> Đăng ký miễn phí tại [dictionaryapi.com](https://dictionaryapi.com) — giới hạn 1 000 query/ngày/key.  
> Cột VI và ZH vẫn hoạt động ngay cả khi chưa có key.

### 3. Build & chạy

Mở `DictionaryTool.sln` bằng Visual Studio 2022+ và nhấn **F5**, hoặc build từ command line:

```bash
msbuild DictionaryTool.UI/DictionaryTool.UI.csproj /p:Configuration=Release
```

---

## 🗂️ Cấu trúc project

```
DictionaryTool.sln
├── DictionaryTool.Core/        # Services, models, repositories (Class Library)
├── DictionaryTool.UI/          # WinForms application
│   ├── App.config.example      # Template cấu hình (copy → App.config)
│   ├── Data/
│   │   ├── stardict/           # Từ điển StarDict EN↔VI (offline)
│   │   ├── cedict.txt.gz       # CC-CEDICT EN↔ZH (offline)
│   │   └── tatoeba/            # Câu ví dụ Tatoeba (offline)
│   └── ...
└── DictionaryTool.Tests/       # Unit tests (NUnit)
```

---

## 🔧 Tech stack

- **WinForms** .NET Framework 4.8 · C# 7.3
- **Guna.UI2** — Dark UI components
- **NAudio** — Phát âm thanh
- **System.Data.SQLite** — Cache tra từ (không tra lại MW nếu đã có)
- **Nguồn dữ liệu**: dictionaryapi.dev · Merriam-Webster · StarDict · CC-CEDICT · Tatoeba

---

## 🐛 Báo lỗi / Góp ý

Mở [issue trên GitHub](https://github.com/ThanhNguyenVN93/frm-dictionary-tool/issues) — mô tả rõ bước tái hiện lỗi và phiên bản Windows là được.

---

## ☕ Ủng hộ tác giả

Nếu thấy hữu ích, bạn có thể [ủng hộ qua Ko-fi](https://ko-fi.com/thanhnguyen150993) hoặc quét mã Momo trong app.

---

## 📄 License

[MIT](LICENSE)
