
/*Queries Espaciales con la librería de NetTopologySuite en .net core*/

declare @MiUbicacion GEOGRAPHY = 'POINT(-69.938988 18.481208)'

SELECT TOP (1000) [Id]
      ,[Nombre]
      ,[Ubicacion].ToString() as Ubicación
	  , Ubicacion.STDistance(@MiUbicacion) as Distancia --la distancia en metros
  FROM [PeliculasApi].[dbo].[SalasDeCine]