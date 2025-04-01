namespace ChameleonGame.Persistence
{
    public class Field
    {
        public readonly Color color;
        private Color chameleonColor;

        public event EventHandler? fieldChanged;

        public Color chameleon 
        {
            get => chameleonColor;
            set
            {
                if (value != chameleonColor)
                {
                    chameleonColor = value;
                    fieldChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool HasEnemyChameleon
        {
            get => color != Color.Empty && chameleon != Color.Empty && color != chameleon;
        }

        public Field(Color color)
        {
            this.color = color;
            this.chameleon = Color.Empty;
        }
    }
}
