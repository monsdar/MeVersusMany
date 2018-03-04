
using Caliburn.Micro;
using MeVersusMany.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace MeVersusMany.UI
{
    class RankingViewModel : Screen
    {
        public ObservableCollection<RankItem> RankedErgList { get; set; }
        //private int numSurroundingRanks = 3; //See TODO below...

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
            
            RankedErgList.Clear();
            for (int index = 0; index < tempSortableList.Count; index++) //we need the index, so use a for-loop instead of foreach
            {
                IErg erg = tempSortableList[index];

                var newRankItem = new RankItem()
                {
                    PositionStr = (index + 1).ToString() + ".",
                    Name = erg.Name,
                    DistanceStr = (erg.Distance - baseDistance).ToString("#.") + " m",
                    Color = new SolidColorBrush(erg.ErgColor)
                };
                RankedErgList.Add(newRankItem);
            }

            /////////
            //TODO: This does not work correctly... find a working implementation... Commented out until further action
            //
            ////trim the list to only show first, last and player-surrounding ergs
            ////get index of the player from previously created tempSortableList (it's already sorted, the position should fit)
            //int playerIndex = tempSortableList.IndexOf(playerErg);
            //if (playerIndex > numSurroundingRanks)
            //{
            //    RankedErgList.RemoveRange(1, playerIndex - numSurroundingRanks);
            //}
            //if (tempSortableList.Count > (playerIndex + numSurroundingRanks))
            //{
            //    RankedErgList.RemoveRange(playerIndex + numSurroundingRanks, tempSortableList.Count - 2);
            //}
            //////////


            //TODO: Add background color depending on current pace vs average pace (indicate when a rower is over/underperforming to let the user know when to attack or defend)

            //TODO: Add some progress bar to better visualize the distance for the user
            //TODO: Should we put this as central view with more details?
        }
    }

    class RankItem
    {
        public string PositionStr { get; set; }
        public string Name { get; set; }
        public string DistanceStr { get; set; }
        public Brush Color { get; set; }
    }
}
