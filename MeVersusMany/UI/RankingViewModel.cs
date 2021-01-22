
using Caliburn.Micro;
using MeVersusMany.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace MeVersusMany.UI
{
    class RankingViewModel : Screen
    {
        public double MaxTotalRange { get; } = 50.0;
        public double MinTotalRange { get; } = 5.0;

        public ObservableCollection<RankItem> RankedErgList { get; set; }
        private int maxErgsInList = 10; //How many ergs should be shown

        public RankingViewModel(List<IErg> recordedErgs)
        {
            RankedErgList = new ObservableCollection<RankItem>();

            UpdateRanking(recordedErgs, 0.0);
        }

        public void PerformUpdate(IErg playerErg, List<IErg> recordedErgs)
        {
            var concatenatedErgs = new List<IErg>();
            concatenatedErgs.Add(playerErg);
            concatenatedErgs.AddRange(recordedErgs);
            UpdateRanking(concatenatedErgs, playerErg.Distance);

            NotifyOfPropertyChange(() => RankedErgList);
        }

        private void UpdateRanking(List<IErg> givenErgs, double baseDistance)
        {
            List<IErg> tempSortableList = new List<IErg>();
            tempSortableList.AddRange(givenErgs);
            tempSortableList.Sort((x, y) => y.Distance.CompareTo(x.Distance));

            //get the range where we want to display the boats in
            var totalRange = Math.Abs(tempSortableList.Last().Distance - tempSortableList.First().Distance);
            if (totalRange > MaxTotalRange) totalRange = MaxTotalRange;
            if (totalRange < MinTotalRange) totalRange = MinTotalRange;

            //NOTE: Instead of updating the actual values we're clearing and repopulating the whole list each frame.
            //      This is very performance heavy and not the best design. See LaneDisplay for the kind of workaround this causes.
            RankedErgList.Clear();
            RankItem player = null;
            for (int index = 0; index < tempSortableList.Count; index++) //we need the index, so use a for-loop instead of foreach
            {
                IErg erg = tempSortableList[index];

                var newRankItem = new RankItem()
                {
                    Erg = erg,
                    Position = index + 1,
                    BaseDistance = baseDistance,
                    TotalRange = totalRange
                };
                RankedErgList.Add(newRankItem);

                if(erg.Name == "Player") //TODO: This relies on the Player-Erg being named "Player"... We shouldn't rely on implementation details we have no control over
                {
                    player = newRankItem;
                }
            }

            //get the player and trim the list around him
            int playerIndex = RankedErgList.IndexOf(player);
            if (playerIndex < 0)
            {
                playerIndex = 0;
            }
            RankedErgList = TrimListAroundIndex(RankedErgList, playerIndex, maxErgsInList, true, true);

            //TODO: Add background color depending on current pace vs average pace (indicate when a rower is over/underperforming to let the user know when to attack or defend)
        }

        public ObservableCollection<RankItem> TrimListAroundIndex(ObservableCollection<RankItem> givenList, int givenIndex, int maxItems, bool keepFirst, bool keepLast)
        {
            if(givenList.Count <= maxItems)
            {
                return givenList;
            }

            ObservableCollection<RankItem> trimmedList = new ObservableCollection<RankItem>();
            trimmedList.Add(givenList[givenIndex]);
            int slotsLeft = maxItems - 1;
            int firstIndex = 0;
            int lastIndex = givenList.Count - 1;

            //if we need to keep the first decrease the slots, we'll add this item in the end then
            if(keepFirst && givenIndex != 0)
            {
                firstIndex = 1;
                slotsLeft--;
            }

            //if we need to keep the last item decrease the slots, we'll add this item in the end then
            if (keepLast && givenIndex != (givenList.Count-1))
            {
                lastIndex--;
                slotsLeft--;
            }
            
            int frontIndex = givenIndex;
            int backIndex = givenIndex;
            bool frontBackSwitch = true;
            while (slotsLeft > 0)
            {
                if (frontBackSwitch)
                {
                    frontBackSwitch = false;
                    frontIndex--;
                    if (frontIndex >= firstIndex)
                    {
                        trimmedList.Insert(0, givenList[frontIndex]);
                        slotsLeft--;
                    }
                }
                else
                {
                    frontBackSwitch = true;
                    backIndex++;
                    if (backIndex <= lastIndex)
                    {
                        trimmedList.Add(givenList[backIndex]);
                        slotsLeft--;
                    }
                }
            }

            //add the first item to the front of the list
            if (keepFirst && givenIndex != 0)
            {
                trimmedList.Insert(0, givenList[0]);
            }

            //add the last item to the back of the list
            if (keepLast && givenIndex != (givenList.Count - 1))
            {
                trimmedList.Add(givenList[givenList.Count - 1]);
            }

            return trimmedList;
        }
    }

    class RankItem
    {
        public IErg Erg { get; set; }
        public int Position { get; set; }
        public double BaseDistance { get; set; }
        public double TotalRange { get; set; }

        public string PositionStr
        {
            get
            {
                return Position.ToString() + ".";
            }
        }
        public string Name
        {
            get
            {
                return Erg.Name;
            }
        }

        public double PaceProgression
        {
            get
            {
                return (Erg.FuturePaceInSecs - Erg.PaceInSecs);
            }
        }

        public double Distance
        {
            get
            {
                return (Erg.Distance - BaseDistance);
            }
        }

        public string DistanceStr
        {
            get
            {
                return (Erg.Distance - BaseDistance).ToString("#.") + " m";
            }
        }
        public Brush Color
        {
            get
            {
                return new SolidColorBrush(Erg.ErgColor);
            }
        }
    }
}
