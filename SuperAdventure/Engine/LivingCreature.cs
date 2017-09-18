using System;
using System.ComponentModel;

namespace Engine
{
    public class LivingCreature : INotifyPropertyChanged
    {
        private int _currentHitPoints;
        public int CurrentHitPoints
        {
            get { return _currentHitPoints; }
            set
            {
                _currentHitPoints = value;
                OnPropertyChanged("CurrentHitPoints");
                OnPropertyChanged("ShowHitPoints");
            }
        }

        private int _maximumHitPoints;
        public int MaximumHitPoints
        {
            get { return _maximumHitPoints; }
            set
            {
                _maximumHitPoints = value;
                OnPropertyChanged("MaximumHitPoints");
                OnPropertyChanged("ShowHitPoints");
            }
        }

        protected void CompletelyHeal()
        {
            CurrentHitPoints = MaximumHitPoints;
        }

        protected void Heal(int hitPointsToHeal)
        {
            CurrentHitPoints = Math.Min(CurrentHitPoints + hitPointsToHeal, MaximumHitPoints);
        }

        public bool IsDead { get { return CurrentHitPoints <= 0; } }

        public string ShowHitPoints
        {
            get { return _currentHitPoints.ToString() + "/" + _maximumHitPoints.ToString(); }
        }

        public LivingCreature(int currentHitPoints, int maximumHitPoints)
        {
            CurrentHitPoints = currentHitPoints;
            MaximumHitPoints = maximumHitPoints;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
