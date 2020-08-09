using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Vector.App.Models
{
    public sealed class ColorsList
    {
        private static Lazy<ColorsList> instance = new Lazy<ColorsList>(() => new ColorsList());

        private readonly List<ColorInfo> colors = new List<ColorInfo>();

        private ColorsList()
        {
            colors = new List<ColorInfo>();

            var colorsProps = typeof(Colors).GetProperties();
            foreach (var colorProps in colorsProps)
            {
                var colorInfo = new ColorInfo
                {
                    Name = colorProps.Name,
                    Color = (Color)colorProps.GetValue(null)
                };

                colors.Add(colorInfo);
            }
        }

        public static ColorsList Instance { get { return instance.Value; } }

        public IEnumerable<ColorInfo> List
        {
            get { return colors; }
        }

        public ColorInfo this[Color color]
        {
            get { return colors.First(c => c.Color == color); }
        }

        public ColorInfo this[string colorName]
        {
            get { return colors.First(c => c.Name == colorName); }
        }
    }
}
