using Android.Views;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V4.App;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Widget;
using ZXing.Mobile;


namespace SlidingTabLayoutTutorial
{
    [Activity(Label = "Mybrary", MainLauncher = true, Icon = "@drawable/MB")]
    public class MainActivity : FragmentActivity
    {
        private ViewPager _mViewPager;
        private SlidingTabScrollView _mScrollView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            MobileBarcodeScanner.Initialize(Application);
            IdbCon t = new DatabaseConnection();
            
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            _mScrollView = FindViewById<SlidingTabScrollView>(Resource.Id.sliding_tabs);
            _mViewPager = FindViewById<ViewPager>(Resource.Id.viewPager);

            _mViewPager.Adapter = new SamplePagerAdapter(SupportFragmentManager);
            _mScrollView.ViewPager = _mViewPager;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.actionbar_main, menu);
            return base.OnCreateOptionsMenu(menu);
        }       
    }

    public class SamplePagerAdapter : FragmentPagerAdapter
    {
        private List<Android.Support.V4.App.Fragment> mFragmentHolder;

        public SamplePagerAdapter (Android.Support.V4.App.FragmentManager fragManager) : base(fragManager)
        {
            mFragmentHolder =
                new List<Android.Support.V4.App.Fragment> {new Fragment1(), new Fragment2(), new Fragment3()};
        }

        public override int Count
        {
            get { return mFragmentHolder.Count; }
        }

        public override Android.Support.V4.App.Fragment GetItem(int position)
        {
            return mFragmentHolder[position];
        }
    }

    public class Fragment1 : Android.Support.V4.App.Fragment
    {
        private Button _refreshWish;
        private ListView _wishList;
        IdbCon con = new DatabaseConnection();

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.Frag1Layout, container, false);
            _refreshWish = view.FindViewById<Button>(Resource.Id.refreshAlt);
            _wishList = view.FindViewById<ListView>(Resource.Id.AltList);
            con.createDatabases();


            RefreshListview();

            _refreshWish.Click += delegate
            {
                RefreshListview();
            };


            _wishList.ItemClick += (sender, e) =>
            {
                var books = RefreshListview();
                con.deleteData("normal", "DELETE FROM Book WHERE Isbn =" + books[e.Position].Isbn);
                //con.deleteData("alt", "DELETE FROM AlternativeISBN WHERE ISBN =" + books[e.Position].Isbn);
                RefreshListview();
            };

            //mTextView = view.FindViewById<TextView>(Resource.Id.textView1);
            //mTextView.Text = "Fragment 1 Class";
            return view;
        }

        public List<WishBook> RefreshListview()
        {
            List<WishBook> books = con.getWishDatabaseInfo("SELECT * FROM wishBook");
            List<string> list = new List<string>();
            if (books != null)
            {
                for (var i = 0; i < books.Count; i++)
                {
                    var item = books[i];
                    list.Add(item.Title);
                }
            }
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(Context, Android.Resource.Layout.SimpleListItem1, objects: list.ToArray());
            _wishList.Adapter = adapter;
            return books;
        }

        public override string ToString() //Called on line 156 in SlidingTabScrollView
        {
            return "Fragment 1";
        }
    }

    public class Fragment2 : Android.Support.V4.App.Fragment
    {
        private Button _refreshDb;
        private ListView _bookList;
        IdbCon _con = new DatabaseConnection();

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.Frag2Layout, container, false);
            _refreshDb = view.FindViewById<Button>(Resource.Id.btnRefreshList);
            _bookList = view.FindViewById <ListView>(Resource.Id.listBooksDB);
            
            //path string for the databasefile
            // create DB path

            _con.createDatabases();
            RefreshListview();

            _refreshDb.Click += delegate
            {
                RefreshListview();
            };

            _bookList.ItemClick += (sender, e) =>
            {
                var books = RefreshListview();
                _con.deleteData("normal", "DELETE FROM Book WHERE Isbn =" + books[e.Position].Isbn);
                _con.deleteData("alt", "DELETE FROM AlternativeISBN WHERE ISBN =" + books[e.Position].Isbn);
                RefreshListview();
            };

            return view;
        }

        public List<Book> RefreshListview()
        {
            List<Book> books = _con.getDatabaseInfo("SELECT * FROM Book");
            List<string> list = new List<string>();
            if (books != null)
            {
                foreach (var item in books)
                {
                    //Book book = new Book(item.Isbn, item.Title, item.Author, item.Edition, item.Publisher, item.Year, item.Length, item.AltIsbnList);
                    list.Add(item.Title);
                }

            }
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(Context, Android.Resource.Layout.SimpleListItem1, objects: list.ToArray());
            _bookList.Adapter = adapter;
            return books;
        }
        
        public override string ToString() //Called on line 156 in SlidingTabScrollView
        {
            return "Boeken";
        }
    }

    public class Fragment3: Android.Support.V4.App.Fragment
    {
        private Button _mButton;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.Frag3Layout, container, false);
            // Somewhere in your app, call the initialization code:

            _mButton = view.FindViewById<Button>(Resource.Id.button1);

            _mButton.Click += async delegate
            {
                var scanner = new MobileBarcodeScanner();
                var result = await scanner.Scan();

                if (result == null) return;

                var intent = new Intent(Activity, typeof(ScanActivity));
                intent.PutExtra("isbn", result.Text);//"9780571308996");//"9783453190085");//
                StartActivity(intent);

            };
             
            return view;
        }

        public override string ToString() //Called on line 156 in SlidingTabScrollView
        {
            return "Scan boek";
        }
    }
    
}

//'Android.Support.V4.Content.ContextCompat.CheckSelfPermission' not found.