namespace Majiang
{
    public interface View
    {
        void kaiju();

        void update(Message paipu = null);

        void redraw();

        void summary(Paipu paipu);

        void say(string type, int lunban);
    }
}