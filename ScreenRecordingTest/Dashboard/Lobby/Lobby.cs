using System;
using System.Collections.ObjectModel;
using System.Linq;
using Chronos.Networking;
using Core;
using Core.Data;
using Core.Networking;
using Ludio.Base;

namespace Ludio.VM
{
    public class Lobbies : ViewModel
    {
        private static Lobbies _i;

        public static Lobbies I { get => _i ?? (_i = new Lobbies()); }

        public ObservableCollection<Lobby> LobbyList { get; private set; } = new ObservableCollection<Lobby>();

        private Lobby _current;
        public Lobby Current
        {
            get => _current;
            set => Set(ref _current, value);
        }

        public void Add(Lobby lobby)
        {
            if(LobbyList.LastOrDefault() != lobby)
                LobbyList.Add(lobby);
        }

        public void Remove(Lobby lobby)
        {
            LobbyList.Remove(lobby);
        }

        public void Open(Channel channel)
        {
            var lobby = LobbyList.FirstOrDefault(x => x.Channel.ID == channel.ID);
            lobby = lobby ?? new Lobby(channel);
            Open(lobby);
        }

        public void Open(Lobby lobby)
        {
            Add(lobby);
            Current?.Leave(User.I);
            lobby.Join(User.I);
            Current = lobby;
            Navigator.I.Goto(Page.Lobby, Current);
        }

        public void Close(Lobby lobby)
        {
            if (LobbyList.LastOrDefault() == lobby)
            {
                lobby.Leave(User.I);
                LobbyList.Remove(LobbyList.Last());
            }

            Current = LobbyList.Count > 0 ? LobbyList.Last() : null;

            Current?.Join(User.I);
            Navigator.I.Goto(Page.Lobby, Current);
        }
    }

    public class Lobby : ViewModel
    {
        public Channel Channel { get; }
        public RelayCommand OpenCommand { get; }
        public RelayCommand CloseCommand { get; }
        public RelayCommand StartCommand { get; }
        public RelayCommand JoinCommand { get; }
        public RelayCommand LeaveCommand { get; }

        public Lobby(Channel c)
        {
            Channel = c;
            OpenCommand = new RelayCommand(Open);
            JoinCommand = new RelayCommand(Join);
            LeaveCommand = new RelayCommand(Leave);
            CloseCommand = new RelayCommand(Close);
            StartCommand = new RelayCommand(Start);

            ServerConnection.I.OnUserReceived += OnUserReceived;
            ServerConnection.I.OnUsersReceived += OnUsersReceived;
        }

        private void OnUsersReceived(object sender, PacketEventArgs e)
        {
            if (Channel.ID != User.I.Channel)
                return;

            OnUI(() =>
            {
                e.Data.Reset();
                e.Data.Read(out int usercount, out bool s);
                Channel.Users.Clear();
                for(int i = 0; i < usercount; i++)
                {
                    e.Data.Read(out byte command, out s);
                    if ((Packet)command == Packet.User)
                        UserReceived(e.Data);
                }
            });
        }

        private void OnUserReceived(object sender, PacketEventArgs e)
        {
            if (Channel.ID != User.I.Channel)
                return;

            OnUI(() =>
            {
                e.Data.Reset();
                UserReceived(e.Data);
            });
        }

        public void UserReceived(IPPacket data)
        {
            var user = new User(data);
            user.ID.Print("User received");
            user.Channel.Print();
            Channel.ID.Print();

            if (user.Channel == Channel.ID)
            {
                Join(user);
            }
            else
            {
                Leave(user);
            }
        }

        public void Open(object obj) { Channel.Open(); }
        public void Join(object obj) { Channel.AddUser(obj as User); }
        public void Leave(object obj) { Channel.RemoveUser(obj as User); }
        public void Close(object obj) { Channel.Close(); }
        public void Start(object obj) { Channel.Start(); }
    }
}