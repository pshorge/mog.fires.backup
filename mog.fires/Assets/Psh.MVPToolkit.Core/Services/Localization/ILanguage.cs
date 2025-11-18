namespace Psh.MVPToolkit.Core.Services.Localization
{
    public interface ILanguage
    {
        public string Name { get; set; }
        public string Tag { get; set; }
        public bool IsDefault { get; set; }
    }
}