using Moq;
using System;
using System.Threading.Tasks;
using TravelBuddy.Calificaciones;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Xunit;

namespace TravelBuddy.Application.Tests.Calificaciones
{
    public class CalificacionAppService_Tests
    {
        private readonly Mock<IRepository<Calificacion, Guid>> _calificacionRepositoryMock;
        private readonly Mock<ICurrentUser> _currentUserMock;
        private readonly CalificacionAppService _appService;

        public CalificacionAppService_Tests()
        {
            _calificacionRepositoryMock = new Mock<IRepository<Calificacion, Guid>>();
            _currentUserMock = new Mock<ICurrentUser>();

            _currentUserMock.Setup(u => u.Id).Returns(Guid.NewGuid());
            _currentUserMock.Setup(u => u.IsAuthenticated).Returns(true);

          //  _appService = new CalificacionAppService(
          //     _calificacionRepositoryMock.Object,
          //      _currentUserMock.Object
          //  );
        }

        [Fact]
        public async Task ShouldCreateCalificacion_WhenUserHasNotRatedBefore()
        {
            var input = new CreateUpdateCalificacionDto
            {
                DestinoId = Guid.NewGuid(),
                Estrellas = 5,
                Comentario = "Un destino increíble, superó mis expectativas."
            };

            var result = await _appService.CreateAsync(input);

            Assert.NotNull(result);
            Assert.Equal(5, result.Estrellas);
            Assert.Equal("Un destino increíble, superó mis expectativas.", result.Comentario);
        }

        [Fact]
        public async Task ShouldReturnAverageRating_WhenDestinoHasRatings()
        {
            var destinoId = Guid.NewGuid();

            var result = await _appService.GetPromedioByDestinoAsync(destinoId);

            Assert.NotNull(result);
            Assert.True(result.TotalCalificaciones >= 0);
            Assert.True(result.Promedio >= 0 && result.Promedio <= 5);
        }

        [Fact]
        public async Task ShouldAllowUpdate_WhenUserIsTheOwnerOfCalificacion()
        {
            var calificacionId = Guid.NewGuid();
            var input = new CreateUpdateCalificacionDto
            {
                DestinoId = Guid.NewGuid(),
                Estrellas = 4,
                Comentario = "Comentario actualizado"
            };

            var result = await _appService.UpdateAsync(calificacionId, input);

            Assert.NotNull(result);
            Assert.Equal(4, result.Estrellas);
        }
    }
}
