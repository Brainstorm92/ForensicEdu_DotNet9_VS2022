using ForensicEdu.Mobile.Core.Services;
using ForensicEdu.Mobile.Core.Models;
namespace ForensicEdu.Mobile.Core.ViewModels;
public sealed class CatalogViewModel(AcademyApi api):ScreenState
{
 public List<Course> Courses{get;private set;}=[];
 public Task LoadAsync()=>RunAsync(async()=>{Courses=await api.GetAsync<List<Course>>("courses",false);Changed(nameof(Courses));});
 public IEnumerable<Course> Search(string text)=>Courses.Where(x=>(x.Title+" "+x.Category).Contains(text,StringComparison.CurrentCultureIgnoreCase));
}
