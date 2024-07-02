using System;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public interface ICanvasStateActions
    {
        public Action OnCanvasActivate
        {
            get;
            set;
        }

        public Action OnCanvasDeactivate
        {
            get;
            set;
        }
        
    }
}