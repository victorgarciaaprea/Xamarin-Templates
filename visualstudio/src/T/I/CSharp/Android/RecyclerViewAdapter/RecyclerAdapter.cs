using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;

namespace $rootnamespace$
{
    class $safeitemname$ : RecyclerView.Adapter
    {
        public event EventHandler<$safeitemname$ClickEventArgs> ItemClick;
        public event EventHandler<$safeitemname$ClickEventArgs> ItemLongClick;
        string[] items;

        public $safeitemname$(string[] data)
        {
            items = data;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            
            //Setup your layout here
            View itemView = null;
            //var id = Resource.Layout.__YOUR_ITEM_HERE;
            //itemView = LayoutInflater.From(parent.Context).
            //       Inflate(id, parent, false);

            var vh = new $safeitemname$ViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var item = items[position];

            // Replace the contents of the view with that element
            var holder = viewHolder as $safeitemname$ViewHolder;
            //holder.TextView.Text = items[position];
        }

        public override int ItemCount =>  items.Length;

        void OnClick($safeitemname$ClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick($safeitemname$ClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class $safeitemname$ViewHolder : RecyclerView.ViewHolder
    {
        //public TextView TextView { get; set; }


        public $safeitemname$ViewHolder(View itemView, Action<$safeitemname$ClickEventArgs> clickListener,
                            Action<$safeitemname$ClickEventArgs> longClickListener) : base(itemView)
        {
            //TextView = v;
            itemView.Click += (sender, e) => clickListener(new $safeitemname$ClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new $safeitemname$ClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class $safeitemname$ClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}