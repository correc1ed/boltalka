using AutoMapper;

namespace boltalka.Application.Abstractions.Mappers;

public abstract class MappingProfile : Profile
{
    protected void CreateMap<TS, TD>(Func<TS, IRuntimeMapper, TD> func)
    {
        Func<TS, IRuntimeMapper, TD> func2 = func;
        CreateMap<TS, TD>().ConvertUsing((TS source, TD _, ResolutionContext resolver) => (source != null) ? func2(source, resolver.Mapper) : _);
    }

    protected void CreateMap<TS1, TS2, TD>(Func<TS1, TS2, IRuntimeMapper, TD> func) where TS1 : TD
    {
        Func<TS1, TS2, IRuntimeMapper, TD> func2 = func;
        CreateMap<(TS1, TS2), TD>().ConvertUsing(((TS1, TS2) source, TD _, ResolutionContext resolver) => (source.Item1 != null && source.Item2 != null) ? func2(source.Item1, source.Item2, resolver.Mapper) : _);
    }
}