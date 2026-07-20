
export interface PublicUserProfileDto {
  userId?: string;
  nombre?: string;
  apellido?: string;
  fotoPerfilUrl?: string;
}

export interface UpdateUserProfileDto {
  nombre?: string;
  apellido?: string;
  fotoPerfilUrl?: string;
  preferencias?: string;
  email?: string;
}
