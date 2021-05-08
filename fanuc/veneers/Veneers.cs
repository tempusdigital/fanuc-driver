﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace fanuc.veneers
{
    public class Veneers
    {
        public Machine Machine
        {
            get { return _machine; }
        }
        
        private Machine _machine;

        public Action<Veneers, Veneer> OnDataChange = (vv, v) => { };
        
        public Action<Veneers, Veneer> OnError = (vv, v) => { };
        
        private List<Veneer> _wholeVeneers = new List<Veneer>();
        
        private Dictionary<dynamic, List<Veneer>> _slicedVeneers = new Dictionary<dynamic, List<Veneer>>();
        
        public Veneers(Machine machine)
        {
            _machine = machine;
        }
        
        public void Slice(dynamic split)
        {
            foreach (var key in split)
            {
                _slicedVeneers[key] = new List<Veneer>();
            }
        }
        
        public void Slice(dynamic sliceKey, dynamic split)
        {
            foreach (var key in split)
            {
                _slicedVeneers[sliceKey+"/"+key] = new List<Veneer>();
            }
        }

        public void Add(Type veneerType, string name)
        {
            Veneer veneer = (Veneer)Activator.CreateInstance(veneerType, new object[] { name });
            veneer.OnChange = (v) => OnDataChange(this, v);
            veneer.OnError = (v) => OnError(this, v);
            _wholeVeneers.Add(veneer);
        }

        public void AddAcrossSlices(Type veneerType, string name)
        {
            foreach (var key in _slicedVeneers.Keys)
            {
                Veneer veneer = (Veneer)Activator.CreateInstance(veneerType, new object[] { name });
                veneer.SetSliceKey(key);
                veneer.OnChange = (v) => OnDataChange(this, v);
                veneer.OnError = (v) => OnError(this, v);
                _slicedVeneers[key].Add(veneer);
            }
        }
        
        public void AddAcrossSlices(dynamic sliceKey, Type veneerType, string name)
        {
            foreach (var key in _slicedVeneers.Keys)
            {
                var key_parts = key.ToString().Split('/');
                if (key_parts.Length == 1)
                    continue;
                
                Veneer veneer = (Veneer)Activator.CreateInstance(veneerType, new object[] { name });
                veneer.SetSliceKey(key);
                veneer.OnChange = (v) => OnDataChange(this, v);
                veneer.OnError = (v) => OnError(this, v);
                _slicedVeneers[key].Add(veneer);
            }
        }

        public dynamic Peel(string name, dynamic input)
        {
            return _wholeVeneers.FirstOrDefault(v => v.Name == name).Peel(input);
        }
        
        public dynamic PeelAcross(dynamic split, string name, dynamic input)
        {
            foreach (var key in _slicedVeneers.Keys)
            {
                dynamic temp_split = split;

                if (split is Array)
                {
                    temp_split = string.Join('/', split);
                }
                
                if (key.Equals(temp_split))
                {
                    foreach (Veneer veneer in _slicedVeneers[key])
                    {
                        if (veneer.Name == name)
                        {
                            return veneer.Peel(input);
                        }
                    }
                }
            }

            return new { };
        }

        public void Mark(dynamic split, dynamic marker)
        {
            foreach (var key in _slicedVeneers.Keys)
            {
                dynamic temp_split = split;

                if (split is Array)
                {
                    temp_split = string.Join('/', split);
                }
                
                if (key.Equals(temp_split))
                {
                    foreach (Veneer veneer in _slicedVeneers[key])
                    {
                        veneer.Mark(marker);
                    }
                }
            }
        }
    }
}