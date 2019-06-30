namespace Rex.Models
{
    public interface IRepresenter<TModel, TView>
        where TView : IView<TModel>
    {
        TModel ToModel(TView view);

        TView ToView(TModel model);
    }
}