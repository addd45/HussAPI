using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Adapter;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HussAPI.Classes
{
    public class MqttHelper:IDisposable
    {
        public const string MQTT_PASS_ENVIRONMENT_NAME = "MQTT_PASS";
        readonly MQTTSettings _mqttSettings;
        IMqttClientOptions _mqttOptions;
        static IMqttClient _mqttClient;
        private readonly ILogger _log;

        public MqttHelper(IOptions<MQTTSettings> mqttSettings, ILogger log)
        {
            _log = log;
            _log.LogDebug("MqttHelper()");

            _mqttSettings = mqttSettings.Value;
            string pass = Environment.GetEnvironmentVariable(MQTT_PASS_ENVIRONMENT_NAME);
            _log.LogInformation("Initializing Mqtt. Host: {0} | User: {1}", _mqttSettings.Host, _mqttSettings.User);
            _mqttClient = InitializeMqtt(_mqttSettings.Host, _mqttSettings.User, pass);
        }

        public async Task Reconnect()
        {
            try
            {
                await _mqttClient.ConnectAsync(_mqttOptions);
                _log.LogInformation("MQTT Connected!");
            }
            catch (MqttConnectingFailedException e)
            {
                _log.LogError("Error connecting to MQTT. Error Type: " + e.ReturnCode.ToString());

                switch (e.ReturnCode)
                {
                    //idk
                    case MQTTnet.Protocol.MqttConnectReturnCode.ConnectionAccepted:
                        break;
                    //protocol issue
                    case MQTTnet.Protocol.MqttConnectReturnCode.ConnectionRefusedUnacceptableProtocolVersion:
                        break;
                    //idk
                    case MQTTnet.Protocol.MqttConnectReturnCode.ConnectionRefusedIdentifierRejected:
                        break;
                    //server problem
                    case MQTTnet.Protocol.MqttConnectReturnCode.ConnectionRefusedServerUnavailable:
                        break;
                    //auth problem
                    case MQTTnet.Protocol.MqttConnectReturnCode.ConnectionRefusedBadUsernameOrPassword:
                    case MQTTnet.Protocol.MqttConnectReturnCode.ConnectionRefusedNotAuthorized:
                        break;
                    default:
                        break;
                }
            }
        }

        public async Task SendData(string data)
        {

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(_mqttSettings.ScoreTopic)
                .WithAtMostOnceQoS()
                .WithPayload(data)
                .Build();
            try
            {
                _log.LogInformation("Sending MQTT Message with topic {0}", _mqttSettings.ScoreTopic);
                _log.LogDebug($"Message: {Environment.NewLine} {data}");
                await _mqttClient.PublishAsync(message);
            }
            catch (Exception e)
            {
                _log.LogError("Error sending MQTT message: {0}", e.Message);
            }
        }

        public async Task SendConfigData(string data)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(_mqttSettings.ScoreConfig)
                .WithExactlyOnceQoS()
                .WithPayload(data)
                .Build();
            try
            {
                _log.LogInformation("Sending MQTT Message with topic {0}", _mqttSettings.ScoreConfig);
                _log.LogDebug($"Message: {Environment.NewLine} {data}");
                await _mqttClient.PublishAsync(message);
            }
            catch (Exception e)
            {
                _log.LogError("Error sending MQTT message: {0}", e.Message);
            }
        }        

        private IMqttClient InitializeMqtt(string addr, string user, string pw)
        {
            var factory = new MqttFactory();
            var ret = factory.CreateMqttClient();
            ret.Disconnected += Mqtt_Disconnected;

            _mqttOptions = new MqttClientOptionsBuilder()
                .WithClientId("BluesScraper")
                .WithTcpServer(addr)
                .WithCredentials(user, pw)
                //.WithCleanSession()
                .Build();

            return ret;
        }

        private async void Mqtt_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            _log.LogWarning("Warning: Mqtt disconnected");

            //Never was connected in the first place
            if (!e.ClientWasConnected)
            {
                throw e.Exception;
            }
            else
            {
                //TODO: Dynamic
                for (int i = 0; i < 3; i++)
                {
                    _log.LogInformation("Attempting to reconnect. Try #{0}", i);
                    await _mqttClient.ConnectAsync(_mqttOptions);
                    if (_mqttClient.IsConnected)
                    {
                        _log.LogInformation("MQTT reconnected!");
                        break;
                    }
                    else { Thread.Sleep(690); }
                }
            }
        }

        public void Dispose()
        {
            _mqttClient.Dispose();
        }

        public bool IsConnected => _mqttClient.IsConnected;
    }
}
