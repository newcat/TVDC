using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace tvdc
{
    /// <summary>
    /// Interaction logic for ViewerRow.xaml
    /// </summary>
    public partial class ViewerRow : UserControl
    {

        public static readonly DependencyProperty badgesProperty = DependencyProperty.Register(
            "badges", typeof(List<Badges.BadgeTypes>), typeof(ViewerRow));
        public List<Badges.BadgeTypes> badges
        {
            get { return (List<Badges.BadgeTypes>)GetValue(badgesProperty); }
            set { SetValue(badgesProperty, value); }
        }

        public ViewerRow()
        {
            InitializeComponent();

            DependencyPropertyDescriptor badgesDesc = DependencyPropertyDescriptor.FromProperty(badgesProperty, typeof(ViewerRow));
            badgesDesc.AddValueChanged(this, new EventHandler((object sender, EventArgs e) => updateBadges()));
        }

        private void updateBadges()
        {

            if (badges == null)
                return;

            List<UIElement> removeList = new List<UIElement>();

            foreach (UIElement uie in mainPanel.Children)
            {
                if (uie is Image && ((Image)uie).Tag.ToString() == "Badge")
                    removeList.Add(uie);
            }

            foreach (UIElement uie in removeList)
            {
                mainPanel.Children.Remove(uie);
            }

            removeList = null;

            if (badges.Contains(Badges.BadgeTypes.SUBSCRIBER) && Badges.hasSubscriberBadge)
                addBadge(Badges.subscriber);

            if (badges.Contains(Badges.BadgeTypes.TURBO))
                addBadge(Badges.turbo);

            if (badges.Contains(Badges.BadgeTypes.MODERATOR))
                addBadge(Badges.moderator);

            if (badges.Contains(Badges.BadgeTypes.BROADCASTER))
                addBadge(Badges.broadcaster);

            if (badges.Contains(Badges.BadgeTypes.GlOBAL_MOD))
                addBadge(Badges.global_mod);

            if (badges.Contains(Badges.BadgeTypes.ADMIN))
                addBadge(Badges.admin);

            if (badges.Contains(Badges.BadgeTypes.STAFF))
                addBadge(Badges.staff);

        }

        private void addBadge(BitmapImage b)
        {
            Image i = new Image();
            i.Source = b;
            i.Width = 18;  //not using b.Width / b.Heigh because the alpha images
            i.Height = 18; //might have different dimensions
            i.VerticalAlignment = VerticalAlignment.Center;
            i.SnapsToDevicePixels = true;
            i.Margin = new Thickness(5, 0, 0, 0);
            i.Tag = "Badge";
            mainPanel.Children.Insert(0, i);
        }

    }
}
