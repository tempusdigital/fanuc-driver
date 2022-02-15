using System;
using System.Threading.Tasks;
using l99.driver.@base;
using l99.driver.fanuc.strategies;

namespace l99.driver.fanuc.handlers
{
    public class FanucMTC: Handler
    {
        public FanucMTC(Machine machine, object cfg) : base(machine, cfg)
        {
            
        }
        
        public override async Task<dynamic?> OnDataArrivalAsync(Veneers veneers, Veneer veneer, dynamic? beforeArrival)
        {
            // only allow focas performance
            if (veneer.GetType().Name != "FocasPerf")
                return null;
            
            dynamic payload = new
            {
                observation = new
                {
                    time =  new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                    machine = veneers.Machine.Id,
                    name = veneer.Name,
                    marker = veneer.Marker
                },
                state = new
                {
                    time = veneer.ArrivalDelta.TotalMilliseconds,
                    data = veneer.LastArrivedValue
                }
            };

            return payload;
        }

        protected override async Task afterDataArrivalAsync(Veneers veneers, Veneer veneer, dynamic? onArrival)
        {
            if (onArrival == null)
                return;
            
            await veneers.Machine.Transport.SendAsync("DATA_ARRIVE", veneer, onArrival);
        }
        
        public override async Task<dynamic?> OnDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? beforeChange)
        {
            dynamic payload = new
            {
                observation = new
                {
                    time =  new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                    machine = veneers.Machine.Id,
                    name = veneer.Name,
                    marker = veneer.Marker
                },
                state = new
                {
                    time = veneer.ChangeDelta.TotalMilliseconds,
                    data = veneer.LastChangedValue
                }
            };

            return payload;
        }

        protected override async Task afterDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? onChange)
        {
            if (onChange == null)
                return;
            
            await veneers.Machine.Transport.SendAsync("DATA_CHANGE", veneer, onChange);
        }
        
        public override async Task<dynamic?> OnStrategySweepCompleteAsync(Machine machine, dynamic? beforeSweepComplete)
        {
            dynamic payload = new
            {
                observation = new
                {
                    time =  new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                    machine = machine.Id,
                    name = "sweep"
                },
                state = new
                {
                    data = new
                    {
                        online = machine.StrategySuccess,
                        healthy = machine.StrategyHealthy
                    }
                }
            };
            
            return payload;
        }
        
        protected override async Task afterSweepCompleteAsync(Machine machine, dynamic? onSweepComplete)
        {
            await machine.Transport.SendAsync("SWEEP_END", null, onSweepComplete);
        }

        public override async Task OnGenerateIntermediateModel(string json)
        {
            await machine.Transport.SendAsync("INT_MODEL", null, json);
        }
    }
}