using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using WebCore.Classes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WebCore.Game
{

    public interface IUpdatable
    {
        void Update();
    }
    public interface IIndex
    {
        int Index { get; set; }
    }

    public static class Game
    {
        public static List<object?> Objects = new List<object?>();
        public static List<object?> ObjectsToAdd = new List<object?>();
        public static int NextID => Objects.Count + ObjectsToAdd.Count;
        public static List<int> ObjectsToRemove = new List<int>();
        private static bool started = false;
        public static object _lock = new object();
        public static object addlock = new object();
        public static object removelock = new object();
        private static DateTime last = DateTime.Now;
        public static int elapsed => (int)(DateTime.Now - last).TotalMilliseconds;
        public static void Start()
        {
            if (started) return;
            started = true;
            Objects.Add(new Bot(Objects.Count));
            Task.Run(Update);
        }
        private static async void Update()
        {
            var milis = (5 - elapsed);
            if (milis < 0) milis = 0;


            Try.Lock(ref Game._lock, () => {
                foreach (object? obj in Objects.Where(o=>o is not null))
                    try
                    {
                        if (obj is IUpdatable p)
                            p.Update();
                    }
                    catch (Exception ex)
                    {

                    }


                Try.Lock(ref addlock, () => 
                {
                    if (Game.ObjectsToAdd.Count > 0)
                    {
                        Game.Objects.AddRange(Game.ObjectsToAdd);
                        Game.ObjectsToAdd.Clear();
                    }
                });

                Try.Lock(ref removelock, () =>
                {
                    if (Game.ObjectsToRemove.Count > 0)
                    {
                        foreach (var i in Game.ObjectsToRemove.ToList())
                            Game.Objects[i]=null;
                                
                        ObjectsToRemove.Clear();
                    }
                });
            });



            last = DateTime.Now;
            await Task.Delay(10-milis);

            Update();
        }
    }
    public class Bullet : IUpdatable,IIndex
    {
        public bool isbullet => true;
        public double dx { get; set; }
        public double dy { get; set; }
        public double x { get; set; }
        public double y { get; set; }
        public int firedby { get; set; }

        private int _index;

        public int Index { get => _index; set => _index = value; }
        private int step = 7;
        public Bullet(double cos, double sin, int index)
        {
            dx = cos;
            dy = -sin;
            dx *= step;
            dy *= -step;
            _index = index;
            Destroy();
        }
        public Bullet(double degrees,int index)
        {
            dx = Math.Cos((degrees / 180) * Math.PI);
            dy = Math.Sin((degrees / 180) * Math.PI);
            dx *= step;
            dy *= -step;
            _index = index;
            Destroy();
        }
        private async void Destroy() {
            await Task.Delay(1000);
            Try.Lock(ref Game.removelock, () =>
            {
                Game.ObjectsToRemove.Add(this.Index);
            });
        }
        public void Update()
        {

            var oldx = x;
            var oldy = y;
            x += dx*(Game.elapsed / 10);
            y += dy*(Game.elapsed / 10);

            foreach (Player p in Game.Objects.Where(o => o is Player p &&
            (
            //this.x >= p.x && this.x <= (p.width + p.x)
            //&&
            //this.y >= p.y && this.y <= (p.height + p.y)
            new Line(oldx, oldy, x, y).square(p.x, p.y, p.width, p.height)
            )))
                if (p.Index != this.firedby)
                {
                    var player = Game.Objects.ElementAt(this.firedby) as Player;
                    if (p.Die(player.Kills))player.Kill();
                    Try.Lock(ref Game.removelock, () => {
                        Game.ObjectsToRemove.Add(this.Index);
                    });
                }
        }
    }
    public class Bot : Player
    {
        private bool choosed;
        private bool running;
        public Bot(int index) : base("_", index)
        {
            this.OnUpdate += (s, e) => 
            {
                List<object?> objs = null;
                Try.Lock(ref Game._lock, () => {
                    objs = Game.Objects.ToList();
                });

                if (
                    objs.Where(o => o is Player p && /*o is not Bot &&*/ !p.dead && (o as IIndex).Index != this.Index)
                    .Select(_ => new Line(centerx, centery, (_ as Player).centerx, (_ as Player).centery))
                    .OrderBy(_ => _.length)
                    .FirstOrDefault()
                is
                    Line l
                )
                {
                    Input[(int)Keys.W] = false;
                    Input[(int)Keys.A] = false;
                    Input[(int)Keys.S] = false;
                    Input[(int)Keys.D] = false;

                    if ((l.length < 420 && !running)||l.length<250)
                    {
                        if (MouseState[1] is null)
                            MouseState[1] = new Mouse() { which = 1, timestamp = DateTime.Now.Ticks, type = "mousedown", x = (float)l.x2, y = (float)l.y2 };
                        else MouseState[0] = new Mouse() { which = 0, timestamp = DateTime.Now.Ticks, type = "mousemove", x = (float)l.x2, y = (float)l.y2 };

                        choosed = false;
                        running = false;
                        Input[(int)Keys.ShiftKey] = false;
                    }
                    else
                    {
                        MouseState[1] = null;
                        var hip = Math.Sqrt((l.lx * l.lx) + (l.ly * l.ly));
                        var cos = -(l.lx / hip);
                        var sin = l.ly / hip;
                        
                        var angle = Math.Acos(cos);
                        var quadrant = angle / Math.PI;
                        var degrees = quadrant * 180;
                        if (l.ly < 0)
                            degrees = 360 - degrees;

                        if (degrees >= 360 - (45*1.5) || degrees <= (45 * 1.5))
                            Input[(int)Keys.D] = true;

                        if (degrees >= 45/2 && degrees <= 90 + (45 * 1.5))
                            Input[(int)Keys.W] = true;

                        if (degrees >= 90+(45/2) && degrees <= 180 + (45 * 1.5) )
                            Input[(int)Keys.A] = true;

                        if (degrees >= 180 + (45/2) && degrees <= 360 - (45/2))
                            Input[(int)Keys.S] = true;

                        if (l.length >= 460)
                        {
                            Input[(int)Keys.ShiftKey] = true; 
                            running=true;
                        }
                        else if (running && !choosed)
                        {
                            
                            running = Random.Shared.Next(1) == 1;
                            Input[(int)Keys.ShiftKey] = running;
                            choosed= true;
                        }
                            
                    }
                }
                else
                {
                    MouseState[1] = null;
                    Input[(int)Keys.ShiftKey] = false;
                }

            };
        }
    }
    public class Player : IUpdatable, IIndex
    {
        public Mouse[] MouseState = new Mouse[6];
        private string _id;
        public Player(string connectionId, int index)
        {
            _id = connectionId;
            _index = index;
            RandomCoordenates();
        }

        private void RandomCoordenates()
        {
            x = 1000 - Random.Shared.Next(2000);
            y = 1000 - Random.Shared.Next(2000);
        }

        public bool isplayer => true;
        public double x { get; set; }
        public double y { get; set; }
        public double centerx => x + (width / 2);
        public double centery => y + (height / 2);
        public int? sequence { get; set; }
        public bool mirrored { get; set; }
        public int width { get; set; } = 26;
        public int height { get; set; } = 50;
        public bool crouched { get; set; }
        public bool dead { get; set; }
        public object? beginAnim { get; set; }
        public string type { get; set; } = "Default";
        private bool _toggle_crouched {get;set; }
        private bool _crouching { get;set; }

        private int _index;
        public int Index { get => _index; set => _index = value; }

        private bool _canshoot = true;

        public int Kills { get; set; }

        private bool CanShoot
        {
            get
            {
                if (_canshoot)
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(235);
                        _canshoot = true;
                    });

                    _canshoot = false;

                    return true;
                }
                return false;
            }
        }


        public bool Id(string connectionId) { return this._id == connectionId; }

        public List<bool?> Input = new List<bool?>(new bool?[254]);
        public async void Shoot(double x, double y) 
        {
            var bulletx = 0d;
            var bullety = 0d;

            var hip = Math.Sqrt((x * x) + (y * y));
            var cos = x / hip;
            var sin = y / hip;

            var angle = Math.Acos(cos);
            var quadrant = angle / Math.PI;
            var degrees = quadrant * 180;
            if (y < 0)
                degrees = 360 - degrees;

            if (x < 0) mirrored = true;
            if (x > 0) mirrored = false;
            var t = 45 / 2;
            var b = 45 / 6;
            if (degrees > t && degrees < (180 - t))
            {//up
                if (crouched)
                    sequence = 12;
                else
                    sequence = 7;
            }
            else
            if (degrees > 180 + b && degrees < 360 - b)
            {//down
                if (crouched)
                    sequence = 11;
                else
                    sequence = 6;
            }
            else//normal
                if (crouched)
                sequence = 10;
                else
                sequence = 5;

            bulletx=(this.x+(this.width/2))+(cos*25);
            bullety=((crouched?0:-5)+this.y+(this.height/2))-(sin*(crouched?25:35));
            if (!crouched)
                degrees += 25 - Random.Shared.Next(50);
            if (!CanShoot) return;
            bool error = true;
            while (error)
                try
                {
                    error = false;
                    lock (Game.addlock)
                    {
                        error = false;
                        Game.ObjectsToAdd.Add(new Bullet(degrees, Game.Objects.Count + Game.ObjectsToAdd.Count) { x = bulletx, y = bullety,firedby=this.Index });
                    }
                }
                catch { error = true; }
        }
        public async void BeginAnim(int sequence, int timeout, Action? d = null)
        {
            BeginAnim(sequence, this.type, timeout, d);
        }
        public async void BeginAnim(int sequence, string type, int timeout, Action? d = null)
        {
            beginAnim = new { sequence=sequence,type=type};
            this.sequence = sequence;
            this.type = type;
            await Task.Delay(timeout);
            beginAnim = null;
            if (d is not null)
                d();
        }
        public event EventHandler OnUpdate;
        public void Update()
        {
            if(dead) return;
            if(OnUpdate is not null)
                OnUpdate(this,new EventArgs());

            bool c  = Input[(int)Keys.C] ?? false;
            if (c)
            {
                if (!_toggle_crouched)
                {
                    _crouching = true;
                    crouched = !crouched;
                    _toggle_crouched = true;
                    sequence = null;
                    string type;
                    if(crouched)
                        type = "Play";
                    else
                        type = "Play Reverse";

                    BeginAnim(8,type,360, () => {
                        _crouching = false;
                        this.type = "default";
                    });

                }
            }
            else
                if (_toggle_crouched)
                    _toggle_crouched = false;

            if(_crouching)
                return;


            if (MouseState[1] != null)
            {
                if (MouseState[1].type == "mousedown")
                {
                    if (MouseState[0] != null && MouseState[0].timestamp >= MouseState[1].timestamp)
                    {
                        var x = MouseState[0].x - (this.x + (this.width / 2));
                        var y = (-1)*(MouseState[0].y - (this.y + (this.height / 2)));
                       
                        this.Shoot(x,y);
                        return;//shoot
                    }
                    else
                    {
                        var x = MouseState[1].x - (this.x + (this.width / 2));
                        var y = (-1) * (MouseState[1].y - (this.y + (this.height / 2)));
                        
                        this.Shoot(x,y);
                        return;//shoot
                    }
                }

            }
            if (crouched)
            {
                sequence = 9;
                return;
            }
            bool a, d, s, w, up,down,left,right,shift;
            a = Input[(int)Keys.A] ?? false;
            d = Input[(int)Keys.D] ?? false;
            s = Input[(int)Keys.S] ?? false;
            w = Input[(int)Keys.W] ?? false;
            up = Input[(int)Keys.Up] ?? false;
            down = Input[(int)Keys.Down] ?? false;
            left = Input[(int)Keys.Left] ?? false;
            right = Input[(int)Keys.Right] ?? false;
            shift = Input[(int)Keys.ShiftKey] ?? false;
            double dx = 0;
            double dy = 0;
            double step = 0.899;
            if (a||left)
            {
                dx -= step;
                this.mirrored = true;
            }
            if (d||right)
            {
                dx += step;
                this.mirrored = false;
            }
            if (s||down)
            {
                dy += step;
            }
            if (w||up)
            {
                dy -= step;
            }
            if(dy!=0&&dx!=0)
            {
                dy = dy / 1.44;
                dx = dx / 1.44;
            }

            if (dy == 0 && dx == 0)
                this.sequence = 4;
            else if (shift)
            {
                this.sequence = 2;
                dx *= 2.4;
                dy *= 2.4;
            }
            else
                this.sequence = 1;

            this.x += dx * (Game.elapsed/10);
            this.y += dy * (Game.elapsed/10);
        }
        public int life { get; set; } = 100;
        public bool Die(int kills)
        {
            if (this.dead) return false;
            life -= Random.Shared.Next(20)*(1+(kills/5));
            if (life > 0) return false;
            this.dead = true;
            this.sequence = 14;
            this.BeginAnim(14, 2000, () => Try.Lock(ref Game.removelock, () => Game.ObjectsToRemove.Add(this.Index)));
            return true;
        }

        internal void Kill()
        {
            this.life+= Random.Shared.Next(10,40);
            if (this.life > 100) this.life = 100;
            this.Kills += 1;
        }
    }
}
