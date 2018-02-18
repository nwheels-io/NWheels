using System;
using System.Threading.Tasks;
using ElectricityBilling.Domain.Sensors;

namespace ElectricityBilling.SensorService.HubIntegrationStub
{
    public class StubSensorHubService : ISensorHubService
    {
        public Task RequestSensorActivationChangeAsync(string sensorId, bool active)
        {
            throw new NotImplementedException();
        }

        public event SensorReadingReceivedHandler SensorReadingReceived;

        private async Task OnSensorReadingReceived(DateTime timestamp, string sensorId, decimal kwh)
        {
            var eventHandlers = this.SensorReadingReceived;
            if (eventHandlers == null)
            {
                return;
            }

            Delegate[] invocationList = eventHandlers.GetInvocationList();
            Task[] handlerTasks = new Task[invocationList.Length];

            for (int i = 0; i < invocationList.Length; i++)
            {
                var handler = (SensorReadingReceivedHandler) invocationList[i];
                handlerTasks[i] = handler(timestamp, sensorId, kwh);
            }

            await Task.WhenAll(handlerTasks);
        }
    }
}
