using Windows.UI;

namespace Domain
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Color Color1 { get; set; }
        public Color Color2 { get; set; }
        public Color TextColor { get; set; }

        public virtual ICollection<Comic> Comics { get; set; } = [];

        public Tag()
        {
            Id = Guid.NewGuid();
            Color1 = Color.FromArgb(255, 0, 0, 0);
            Color2 = Color.FromArgb(255, 0, 0, 0);
            TextColor = Color.FromArgb(255, 255, 255, 255);
        }
    }
}
