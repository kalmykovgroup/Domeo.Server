using Materials.Contracts.DTOs;
using MediatR;

namespace Materials.Application.Queries.Search;

public sealed record GetSearchSuggestionsQuery(
    string Query,
    int Limit = 10) : IRequest<List<SearchSuggestionDto>>;
