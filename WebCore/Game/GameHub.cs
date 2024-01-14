using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Data;
using WebCore.Game;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using WebCore.Classes;

namespace WebCore.Game
{
    public class Mouse {
        public int? which { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public string? type { get; set; }
        public float timestamp { get; set; }
    }
    public class GameHub : Hub
    {
        private static DateTime last = DateTime.Now;
        public static int elapsed => (int)(DateTime.Now - last).TotalMilliseconds;
        public async IAsyncEnumerable<object> Update([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            while (true)
            {
                var milis = (5 - elapsed);
                if (milis < 0) milis = 0;
                List<Object> list=null;
                cancellationToken.ThrowIfCancellationRequested();
                while (list == null)
                try
                {
                    lock (Game._lock)
                    {
                        list = Game.Objects.ToList();
                    }
                }
                catch { list = null; }
                yield return list.Where(o=>o is not null).ToList();
                await Task.Delay(20);
            }
        }
        public async Task<object> Bot()
        {
            int? id=null;
            Try.Lock(ref Game.addlock,()=>{
                id = Game.NextID;
                Game.ObjectsToAdd.Add(new Bot(Game.NextID));
            });
            return id;
        }
        public async Task<object> Enter()
        {
            Game.Start();
            int? id = null;

            Try.Lock(ref Game._lock, () => {
                Player p = (Player)Game.Objects.FirstOrDefault(_ => _ is Player p && p.Id(Context.ConnectionId));
                if (p is null)
                {
                    Game.Objects.Add(new Player(Context.ConnectionId, Game.Objects.Count));
                    id = Game.Objects.Count - 1;
                }
                else
                {
                    id = p.Index;
                }

                }
            );
            return id;
        }

        public async void Input(bool?[] input)
        {
            List<object> objects=null;

            Try.Lock(ref Game._lock, () => { 
                objects = Game.Objects.ToList(); 
            });

            Player player = (Player)objects.FirstOrDefault(o => o is Player p ? p.Id(Context.ConnectionId) : false);
            if (player is null) return;
            var e = 0;
            foreach (var i in input)
            {

                if (e > player.Input.Count - 1)
                    player.Input.Insert(e, i);
                else
                    player.Input[e] = i;

                e++;
            }

        }
        
        public async void Mouse(Mouse[] mouse)
        {
            List<object> objects = null;

            Try.Lock(ref Game._lock, () => {
                objects = Game.Objects.ToList();
            });

            Player player = (Player)objects.Where(o=> o is Player).FirstOrDefault(o => (o as Player).Id(Context.ConnectionId));
            if (player is null) return;
            var i = 0;

            foreach( Mouse m in mouse)
            {
                player.MouseState[i] = m;
                i++;
            }
        }
    }
}
