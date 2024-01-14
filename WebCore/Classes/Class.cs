namespace WebCore.Classes
{
    public static class Try
    {
        public static void Lock(ref object Lock, Action Try)
        {
            while (true)
            {
                try
                {
                    lock (Lock)
                    {
                        Try();

                        return;
                    }
                }
                catch { }
            }
        }
    }




    public class Line{
        public double x1;
        public double y1;
        public double x2;
        public double y2;
        public Line(double x1,double y1, double x2, double y2)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
        }
        public double lx => x1 - x2;
        public double ly => y1 - y2;
        public double length => Math.Sqrt((lx*lx) + Math.CopySign(ly*ly,lx*lx));
        public double top => y1<=y2?y1:y2;
        public double bottom => y1>=y2?y1:y2;
        public double left => x1<=x2?x1:x2;
        public double right => x1 >= x2 ?x1:x2;
        public bool square(double x,double y,double width,double height) 
        {
            bool horC = (x <= left && (x + width) >= left) || (x <= right && (x + width) >= right);
            bool horT = (y <= top && (y + height) >= top) || (y <= bottom && (y + height) >= bottom);
            if (!(horC && horT)) return false;


            if (line(x, y, x, y + height))//left
                return true;
            if (line(x + width, y, x + width, y + height))//right
                return true;
           if(line(x, y, x + width, y))//top
                return true;
            if (line(x, y + height, x + width, y + height))//bottom
                return true;

            return false;
        }
        public bool line(double x3, double y3, double x4, double y4)
        {
            double uA = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));

            double uB = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));

            if (uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1)
            {
                return true;
            }
            return false;

            
        }
        public bool line(Line line)
        {
            double x3 = line.x1;
            double y3 = line.y1;
            double x4 = line.x2;
            double y4 = line.y2;
            return this.line(x3, y3, x4, y4);
        }
        
    }
}
