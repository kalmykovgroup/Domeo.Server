using Materials.Abstractions.DTOs;
using MediatR;

namespace Materials.Abstractions.Queries.Search;

public sealed record GetSearchSuggestionsQuery(
    string Query,
    int Limit = 10) : IRequest<List<SearchSuggestionDto>>;
