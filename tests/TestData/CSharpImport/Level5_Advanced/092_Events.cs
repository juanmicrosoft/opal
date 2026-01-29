namespace Events
{
    using System;

    public class Button
    {
        public event EventHandler Clicked;

        public void Click()
        {
            if (Clicked != null)
            {
                Clicked(this, EventArgs.Empty);
            }
        }
    }

    public class CustomEventArgs : EventArgs
    {
        public string Message { get; set; }
    }

    public class Publisher
    {
        public event EventHandler<CustomEventArgs> MessageSent;

        public void Send(string message)
        {
            if (MessageSent != null)
            {
                MessageSent(this, new CustomEventArgs { Message = message });
            }
        }
    }
}
