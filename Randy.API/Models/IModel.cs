using Randy.API.Views;

namespace Randy.API.Models
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