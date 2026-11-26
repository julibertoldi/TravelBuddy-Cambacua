
export interface CityDetailDto extends CityDto {
  region?: string;
  latitude: number;
  longitude: number;
  population: number;
}

export interface CityDto {
  id: number;
  wikiDataId?: string;
  name?: string;
  country?: string;
  region?: string;
  population: number;
  latitude: number;
  longitude: number;
  imageUrl?: string;
}

export interface CitySearchRequestDto {
  partialName?: string;
  pais?: string;
  poblacionMinima?: number;
}

export interface CitySearchResultDto {
  cities: CityDto[];
}
