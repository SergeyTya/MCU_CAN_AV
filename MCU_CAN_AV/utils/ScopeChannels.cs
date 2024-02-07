using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace utils
{
    internal class ScopeChannels: List<Channel>
    {
        private const int ch_num_max = 6;
        private int _chanel_capacity = 240;
        private int chnls = 1;

        public int ChannelCapacity
        {
            set
            {
                _chanel_capacity = value;
                this.Clear();
                for (int i = 0; i < chnls; i++)
                {
                    this.Add(new Channel(_chanel_capacity));
                }
            }
            get { return _chanel_capacity; }
        }

        public ScopeChannels(int chnl):base()
        {
            chnls = chnl;
            for (int i = 0; i < chnl; i++)
            {
                this.Add(new Channel(_chanel_capacity));
            }
        }

        private void change_cannels(Action<Channel, double> action, double[] newValue = null)
        {
            int i = 0;
            foreach (Channel ch in this)
            {
                double tmp = 0;
                if (newValue != null)
                {
                    if (i > newValue.Length - 1) return;
                    tmp = newValue[i++];
                }
                action(ch, tmp);
                if (i > this.Count - 1)      return;
            }
        }

        public void SetGains(double[] newValue)
        {
            change_cannels( (_,__) => _.gain = __ , newValue);
        }

        public void ResetGainsOffsets()
        {
            change_cannels((_, __) => {
                _.gain = 0;
                _.offset = 0;
            });
        }


        public void SetOffsets(double[] newValue)
        {
            change_cannels((_, __) => _.offset = __, newValue);
        }

        public void AddMissingFrame()
        {
            change_cannels((_, __) => {
                _.gain = 0;
                _.offset = 0;
            });
        }

        public void Update(double[] Ypoints)
        {
            change_cannels((_, __) => _.AddPoint(__), Ypoints);
        }
    }

    internal class Channel
    {
        public double gain = 1;
        public double offset = 0;

        private double[] _data;

        public Channel(int capacity)
        {
            this.Capacity = capacity;
        }

        public int Capacity { set { _data = new double[value]; } }

        public double[] Points { get { return _data; } }

        public void AddPoint(double y)
        {
            Array.Copy(_data, 1, _data, 0, _data.Length - 1);
            _data[^1] = y * gain + offset;
        }

        public void Clear(double y)
        {
            Capacity = _data.Length;
        }
    }
}
