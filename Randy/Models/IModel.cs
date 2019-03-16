using Randy.Views;

namespace Randy.Models
{
    public interface IModel
    {

    }

    public interface IModel<TModel> : IModel
        where TModel : IModel
    {
        T ToView<T>() where T : IModelView<TModel>, new();
    }
}