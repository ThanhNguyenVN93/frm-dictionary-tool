============================================================
 Dictionary Tool — Hướng dẫn cài đặt
 Tra từ điển Anh · Việt · Trung trong một cửa sổ (EN / VI / ZH)
============================================================

Thư mục này chứa đầy đủ chương trình. Có 2 cách dùng:

------------------------------------------------------------
1) CÀI ĐẶT  —  chạy  installer.bat
------------------------------------------------------------
- Cài vào:  %LOCALAPPDATA%\Dictionary Tool
- Không cần quyền Administrator.
- Tự tạo shortcut ngoài Desktop và trong Start Menu.
- Gỡ cài đặt: chạy  uninstall.bat  trong thư mục đã cài
  (hoặc xoá thủ công thư mục trên + 2 shortcut).

------------------------------------------------------------
2) BẢN PORTABLE  —  chạy  portable.bat
------------------------------------------------------------
- Tạo thư mục "Dictionary Tool - Portable" ngay cạnh đây,
  chạy liền, KHÔNG đụng chạm hệ thống (không shortcut hệ
  thống, không registry).
- Cấu hình (theme, cỡ chữ, phím tắt, bookmark...) lưu ngay
  cạnh file .exe nên mang qua USB / máy khác vẫn giữ nguyên.

------------------------------------------------------------
LƯU Ý
------------------------------------------------------------
- Lần đầu mở, Windows SmartScreen có thể cảnh báo (file chưa
  ký số). Chọn "More info" -> "Run anyway" để chạy.
- Cần giữ các thư mục  Data\ , x86\ , x64\  cạnh file .exe —
  đây là dữ liệu từ điển offline và engine SQLite.
- Phím tắt gọi nhanh mặc định: Ctrl + Shift + D.

------------------------------------------------------------
DÀNH CHO NGƯỜI PHÁT HÀNH (tuỳ chọn)
------------------------------------------------------------
- Muốn gói thành 1 file setup.exe duy nhất: cài Inno Setup rồi
  build file  installer\DictionaryTool.iss  (xem hướng dẫn trong
  file đó). Kết quả: installer\Output\DictionaryTool-Setup.exe
