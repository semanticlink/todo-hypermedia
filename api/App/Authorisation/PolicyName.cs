using System;
using Domain.Models;

namespace App.Authorisation
{
    public class PolicyName
    {
        public RightType Type { get; set; }
        public Permission Access { get; set; }
        public string ResourceKey { get; set; } = "id";

        public string Serialise()
        {
            return $"{Type}:{Access}:{ResourceKey}";
        }

        public static PolicyName Deserialise(string encodedPolicy)
        {
            var str = encodedPolicy.Split(':');
            var access = Enum.Parse(typeof(Permission), str[1]);
            var type = Enum.Parse(typeof(RightType), str[0]);
            return Make((RightType) type, (Permission) access, str[2]);
        }

        public static PolicyName Make(RightType type, Permission access, string resourceKey)
        {
            return new PolicyName
            {
                Type = type,
                Access = access,
                ResourceKey = resourceKey
            };
        }
    }
}