﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using uoplib.mvvm;
using Xamarin.Forms;


// https://docs.microsoft.com/xamarin/xamarin-forms/user-interface/listview/
// https://docs.microsoft.com/xamarin/xamarin-forms/user-interface/listview/interactivity
// https://docs.microsoft.com/dotnet/api/xamarin.forms.listview?view=xamarin-forms

namespace SimpleListView
{
    public class MainPageViewModel : ViewModelBase
    {
        //**********************  PRIVATE MEMBER VARIABLES *********************

        private IMainPageHelper _viewHelper;
        private ObservableCollection<PlanetGroup> _planetGroups;
        private string _titleString = "Nothing Selected";
        private SolPlanet _selectedPlanet;

        // ***********************  BINDABLE PROPERTIES ************************

        // ItemSource binds to an IEnumerable
        public ObservableCollection<PlanetGroup> PlanetGroups {
            get => _planetGroups;

            set
            {
                if (_planetGroups == value) return;
                _planetGroups = value;
                OnPropertyChanged();
            }
        }

        // Title string - displaying currently selected planet
        public string TitleString {
            get => _titleString ?? "Nothing Selected";
            set => Update<string>(ref _titleString, value);
        }

        // This property is updated if the ListView selection changes by any means but ONLY if the selection changes
        public SolPlanet SelectedPlanet
        {
            get => _selectedPlanet;
            set
            {
                if (_selectedPlanet == value) return;

                _selectedPlanet = value;
                OnPropertyChanged();

                //Update UI
                TitleString = _selectedPlanet?.Name ?? "Nothing Selected";
            }
        }

        // Back Button Title
        public string BackButtonTitle { get; private set; } = "Cancel";

        // Menu Item Commands
        public ICommand DeleteCommand { get; private set; }
        public ICommand SwapCommand   { get; private set; }
        public ICommand EditCommand   { get; private set; }
        public ICommand AddCommand { get; private set; }

        // ************************    NAVIGATION    ***************************
        private void EditPlanet(SolPlanet p)
        {
            PlanetDetailViewModel vm = new PlanetDetailViewModel(p, this.Navigation);
            PlanetDetailPage detailPage = new PlanetDetailPage(vm);
            _ = Navigation.PushAsync(detailPage);
        }

        private void AddPlanet()
        {
            SolPlanet p = new SolPlanet("New", 5000, false);
            EditPlanet(p);
        }

        // ************************  DATA OPERATIONS ***************************

        // Menu item event - delete
        public void DeleteItem(SolPlanet p) => groupWithPlanet(p).group?.Remove(p);

        // Menu item - swap
        public void SwapItem(SolPlanet planet)
        {
            planet.ToggleExplored();
            SortIntoGroup(planet);

            //Update display
            _viewHelper.ScrollToObject(planet);
        }

        // Find which collection contains a specific data item 
        private (PlanetGroup group, int index) groupWithPlanet(SolPlanet p)
        {
            int grpIndex = 0;
            foreach (PlanetGroup grp in PlanetGroups)
            {
                if (grp.Contains(p)) return (grp, grpIndex);
                grpIndex++;
            }
            return (null, grpIndex);
        }

        // Sort into the correct group
        private void SortIntoGroup(SolPlanet p)
        {
            PlanetGroup explored   = PlanetGroups[0];
            PlanetGroup unexplored = PlanetGroups[1];
            if (p.Explored)
            {
                RemovePlanetFromGroup(p, unexplored);
                AddPlanetToGroup(p, explored);
            } else
            {
                RemovePlanetFromGroup(p, explored);
                AddPlanetToGroup(p, unexplored);
            }
        }

        private void AddPlanetToGroup(SolPlanet p, PlanetGroup grp) {
            if (!grp.Contains(p))
            {
                grp.Add(p);
            }
        }
        private void RemovePlanetFromGroup(SolPlanet p, PlanetGroup grp) => grp.Remove(p);

        // ***************************  CONSTRUCTOR ****************************
        public MainPageViewModel(IMainPageHelper viewHelper) : base(viewHelper.NavigationProxy)
        {
            _viewHelper = viewHelper;

            //Collection of collections
            PlanetGroups = new ObservableCollection<PlanetGroup>()
            {
                new PlanetGroup("Explored", "E") {
                    new SolPlanet("Earth", 147.1, explored:true),
                    new SolPlanet("Mars", 238.92, explored:true)
                },
                new PlanetGroup("Unexplored","U") {
                    new SolPlanet("Mercury", 69.543),
                    new SolPlanet("Venus", 108.62),
                    new SolPlanet("Jupiter", 782.32),
                    new SolPlanet("Saturn", 1498.3),
                    new SolPlanet("Uranus", 2964.2),
                    new SolPlanet("Pluto", 5906.4)
                }
            };

            // ******************** COMMANDS ********************
            DeleteCommand = new Command<SolPlanet>(execute: (p) => DeleteItem(p));
            SwapCommand = new Command<SolPlanet>(execute: (p) => SwapItem(p));
            EditCommand = new Command<SolPlanet>(execute: (p) => EditPlanet(p));
            AddCommand = new Command(execute: () => AddPlanet());

            // ******************* CALL BACKS *******************
            MessagingCenter.Subscribe<PlanetDetailViewModel, SolPlanet>(
                this, "PlanetUpdated",
                callback: (sender, p) => SortIntoGroup(p));
        }

        public MainPageViewModel() : base(null) => throw new System.Exception("You cannot call the parameterless constructor");

    } //END OF CLASS
} //END OF NAMESPACE
