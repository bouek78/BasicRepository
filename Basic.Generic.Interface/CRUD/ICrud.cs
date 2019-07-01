using Basic.Generic.Interface.Pager;
using Basic.Generic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic.Generic.Interface.CRUD
{
    /// <summary>
    /// Le nom Facade est historique, je l'ai gardé
    /// Cette interface permet l'implémentation des manager et des repository côté TModel
    /// </summary>
    /// <typeparam name="TPoco"></typeparam>
    public interface IBasicCrud<TPoco>
        where TPoco : ModelWithId
    {
        Guid Insert(TPoco data);
        void Update(TPoco data);
        void Put(TPoco data);
        void Delete(TPoco data, bool realDelete = false);
        void DeleteById(Guid id, bool realDelete = false);
        List<TPoco> Get(bool includeDelete = false);
        int Count();
        TPoco Get(Guid id);
        IEnumerable<TPoco> Get(IEnumerable<Guid> ids);

    }

    public interface IBasicPager<TPoco> : IBasicCrud<TPoco>
        where TPoco : ModelWithId
    {
        IPagerList<TPoco> GetPager(IPagerQuery query);
    }

    public interface IBasicPager<TPoco, TViewModel> : IBasicCrud<TPoco>
    where TPoco : ModelWithId
    where TViewModel : ModelWithId
    {
        IPagerList<TViewModel> GetPager(IPagerQuery query);
    }
}
