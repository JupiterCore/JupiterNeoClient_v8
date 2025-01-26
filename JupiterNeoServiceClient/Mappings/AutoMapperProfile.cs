using AutoMapper;
using JupiterNeoServiceClient.Models.Domain;
using JupiterNeoServiceClient.Models.Dto.Backup;
using JupiterNeoServiceClient.Models.Dto.File;
using BackupFile = JupiterNeoServiceClient.Models.Domain.BackupFile;

namespace JupiterNeoServiceClient.Mappings
{
    public class AutoMapperProfile : Profile
    {

        public AutoMapperProfile()
        {

            /**
             * Backup Mappings
             **/
            CreateMap<Backup, BackupDto>().ReverseMap();
            CreateMap<Backup, AddBackupDto>().ReverseMap();


            /**
             * File Mappings
             **/
            CreateMap<BackupFile, FileDto>().ReverseMap();
            CreateMap<BackupFile, AddFileDto>().ReverseMap();

        }

    }
}
