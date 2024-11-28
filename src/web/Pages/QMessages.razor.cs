using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using StorageLibrary.Common;
using System.Text;

namespace web.Pages
{
	public partial class QMessages : BaseComponent
	{
		[Parameter]
		public string? CurrentQueue { get; set; }

		public bool ShowTable { get; set; } = false;
		public bool IsBase64 { get; set; } = true;

		public string? NewMessage { get; set; }

		List<PeekedMessageWrapper> AzureQueueMessages = new List<PeekedMessageWrapper>();

		protected override async Task OnParametersSetAsync()
		{
			await LoadMessages();
		}

		private async Task LoadMessages()
		{
			if (string.IsNullOrEmpty(CurrentQueue))
				return;

			try
			{
				Loading = true;
				ShowTable = false;
				AzureQueueMessages.Clear();
				foreach (var msg in await AzureStorage!.Queues.GetAllMessagesAsync(CurrentQueue))
					AzureQueueMessages.Add(msg);

				ShowTable = true;
				Loading = false;
			}
			catch (Exception ex)
			{
				HasError = true;
				ErrorMessage = ex.Message;
			}
		}

		public async Task DeleteQueue()
		{
			try
			{
				await AzureStorage!.Queues.DeleteAsync(CurrentQueue!);
				await Parent!.SelectionDeletedAsync();
			}
			catch (Exception ex)
			{
				HasError = true;
				ErrorMessage = ex.Message;
			}
		}

		public async Task AddMessage()
		{
			try
			{
				if (string.IsNullOrEmpty(NewMessage))
					return;
				var msgToPass = string.Empty;
				if (IsBase64)
					msgToPass = Convert.ToBase64String(Encoding.UTF8.GetBytes(NewMessage));
				else
					msgToPass = NewMessage;
				await AzureStorage!.Queues.CreateMessageAsync(CurrentQueue, msgToPass);
				NewMessage = string.Empty;
				await LoadMessages();
			}
			catch (Exception ex)
			{
				HasError = true;
				ErrorMessage = ex.Message;
			}
		}

		public async Task DeleteMessage()
		{
			try
			{
				await AzureStorage!.Queues.DequeueMessage(CurrentQueue);
				await LoadMessages();
			}
			catch (Exception ex)
			{
				HasError = true;
				ErrorMessage = ex.Message;
			}
		}

	}
}