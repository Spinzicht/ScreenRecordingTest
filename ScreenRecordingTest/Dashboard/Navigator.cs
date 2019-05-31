using System;
using Core.Data;
using Ludio.Base;

namespace Ludio.VM
{
    public enum Page
    {
        Overview,
        Lobby
    }

    public class Navigator : ViewModel
    {
        public static Navigator I { get; private set; } = new Navigator();

        public MainMenu MainMenu { get; private set; } = new MainMenu();

        private ViewModel _current;
        public ViewModel SelectedViewModel
        {
            get => _current;
            private set => Set(ref _current, value);
        }

        private Navigator() {
            SelectedViewModel = Overview.I;
        }

        public override void Init()
        {
            base.Init();
            MainMenu.Init();
            SelectedViewModel.Init();
        }

        public bool IsCurrent(Page page, Channel channel = null)
        {
            switch (page)
            {
                case Page.Overview:
                    return SelectedViewModel == Overview.I;
                case Page.Lobby:
                    return (SelectedViewModel as Lobby)?.Channel.ID == channel.ID;
            }
            return false;
        }

        internal void Goto(Page page, ViewModel vm = null)
        {
            if (vm == null) page = Page.Overview;
            switch (page)
            {
                case Page.Overview:
                    SelectedViewModel = Overview.I;
                    break;
                case Page.Lobby:
                    SelectedViewModel = vm;
                    break;
            }
            MainMenu.Update();
        }
    }
}