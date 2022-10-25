﻿using AutoMapper;
using TibiaEnemyOtherCharactersFinderApi.Dtos;
using TibiaEnemyOtherCharactersFinderApi.Entities;
using TibiaEnemyOtherCharactersFinderApi.Models;

namespace TibiaEnemyOtherCharactersFinderApi
{
    public class TibiaCharacterFinderMapperProfile : Profile
    {

        public TibiaCharacterFinderMapperProfile()
        {
            CreateMap<Character, CharacterResult>();

            CreateMap<CharacterCorrelation, WorldCorrelationDto>();

            CreateMap<World, WorldDto>();

            CreateMap<WorldScan, WorldScanDto>()
            .ForMember(m => m.World, c => c.MapFrom(s => s.World.Name));

            CreateMap<CreateWorldDto, World>();


        }
    }
}
