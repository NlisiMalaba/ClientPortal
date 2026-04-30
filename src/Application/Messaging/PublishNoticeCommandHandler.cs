using Application.Abstractions;
using Application.Messaging.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Messaging;

public sealed class PublishNoticeCommandHandler : IRequestHandler<PublishNoticeCommand, Result<Guid>>
{
    private readonly INoticeRepository _noticeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PublishNoticeCommandHandler(
        INoticeRepository noticeRepository,
        IUnitOfWork unitOfWork)
    {
        _noticeRepository = noticeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(PublishNoticeCommand request, CancellationToken cancellationToken)
    {
        DateTime publishedAt = DateTime.UtcNow;
        Notice notice = Notice.Create(
            id: Guid.CreateVersion7(),
            title: request.Title,
            content: request.Content,
            publishedAt: publishedAt,
            expiresAt: request.ExpiresAt,
            isActive: true,
            targetClientIds: request.TargetClientIds);

        _noticeRepository.Add(notice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(notice.Id);
    }
}
