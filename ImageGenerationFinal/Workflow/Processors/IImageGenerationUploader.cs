using System.Threading.Tasks;

namespace ImageGenerationFinal.Workflow.Processors
{
	public interface IImageGenerationUploader
	{
		Task Process();
	}
}
