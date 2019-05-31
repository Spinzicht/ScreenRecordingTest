using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

using Chronos;
using Core.Data;
using Ludio.Base;

namespace Ludio.VM
{
    public class MainMenu : ViewModel
    {
        public RelayCommand OwnChannelCommand { get; private set; }
        
        public ObservableCollection<Tab> Tabs { get; private set; } = new ObservableCollection<Tab>();

        public Tab Current
        {
            get => Tabs.FirstOrDefault(x => x.IsCurrent);
        }

        public MainMenu()
        {
            Add(new Tab("Ludio"));
            OwnChannelCommand = new RelayCommand(OwnChannelClicked);
        }

        private void OwnChannelClicked(object obj)
        {
            Channel c = new Channel(User.I, 10);
            Add(c).Open();
        }

        public void Update()
        {
            var channel = (Navigator.I.SelectedViewModel as Lobby)?.Channel;
            Add(channel);

            Tabs.ForEach(x => x.Update());
            Notify(nameof(Current));
        }

        private bool HasChannel(Channel c)
        {
            return c != null && Tabs.Any(x => x?.Channel?.ID == c.ID);
        }

        public Tab Add(Channel c)
        {
            Tab tab = null;
            if (c != null)
            {
                if (!HasChannel(c))
                {
                    tab = new Tab(c);
                    Tabs.Add(tab);
                }
                else
                {
                    tab = Tabs.FirstOrDefault(x => x.Channel?.ID == c.ID);
                }
            }
            return tab;
        }

        public Tab Add(Tab t)
        {
            if (!HasChannel(t.Channel))
                Tabs.Add(t);
            return t;
        }

    }

    public class Tab : ViewModel
    {
        private static int COUNT = 0;

        private int _id;
        private Page _page = Page.Overview;

        public Channel Channel { get; private set;}

        public RelayCommand OpenCommand { get; private set; }

        private Thickness _firstActiveTab = new Thickness(0, 1, 1, 0);
        private Thickness _activeTab = new Thickness(1, 1, 1, 0);
        private Thickness _inactiveTab = new Thickness(0, 0, 0, 1);

        public Thickness Border
        {
            get => IsCurrent ? _activeTab : _inactiveTab;
        }

        public bool IsCurrent
        {
            get => Navigator.I.IsCurrent(_page, Channel);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public Tab(Channel c) : this(c?.Name)
        {
            Channel = c;
            _page = Page.Lobby;
        }

        public Tab(string name)
        {
            Name = name ?? "Unknown";
            OpenCommand = new RelayCommand(Open);
            SetID();
        }

        public void Open(object obj = null)
        {
            if(Channel != null)
                Lobbies.I.Open(Channel);
            else Navigator.I.Goto(Page.Overview);
        }

        private void SetID()
        {
            _id = COUNT;
            COUNT++;
            if (_id == 0)
                _activeTab = _firstActiveTab;
        }

        public void Update()
        {
            Notify(nameof(Border));
        }
    }
}
