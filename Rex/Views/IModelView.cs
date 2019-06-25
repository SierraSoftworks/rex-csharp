using Rex.Models;

namespace Rex.Views
{
    public interface IModelView<in T>
        where T : IModel
    {
        void FromModel(T model);
    }

    public interface IModelSource<out T>
        where T : IModel
    {
        T ToModel();
    }
}