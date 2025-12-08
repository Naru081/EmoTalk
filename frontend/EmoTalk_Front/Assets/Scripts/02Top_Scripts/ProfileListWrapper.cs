using System;
using System.Collections.Generic;

[Serializable]
public class ProfileListWrapper
{
    public List<ProfileData> profiles;

    public ProfileListWrapper()
    {
        profiles = new List<ProfileData>();
    }
}