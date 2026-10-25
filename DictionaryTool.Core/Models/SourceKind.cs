namespace DictionaryTool.Core.Models
{
    /// <summary>
    /// Nguồn dữ liệu đã tạo ra một kết quả, để UI hiển thị badge "online/offline"
    /// và tên nguồn — giúp người dùng biết mức độ tin cậy (vd offline StarDict vs
    /// Google Translate tự động).
    /// </summary>
    public enum SourceKind
    {
        None = 0,
        OfflineStarDict,
        OfflineCcCedict,
        OnlineDictApi,
        OnlineWiktionary,
        OnlineGoogleTranslate,
        Cache
    }
}
