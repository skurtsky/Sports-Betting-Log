$(document).ready(function () {
    // Variables
    const searchToggle = $('#searchToggle');
    const searchContainer = $('#searchContainer');
    const searchInput = $('#searchInput');
    const searchResults = $('#searchResults');
    let searchTimeout;
    
    // Toggle search visibility when search icon is clicked
    searchToggle.on('click', function (e) {
        e.preventDefault();
        searchContainer.toggleClass('show');
        if (searchContainer.hasClass('show')) {
            searchInput.focus();
        } else {
            // Clear search when hiding
            searchInput.val('');
            searchResults.empty();
        }
    });

    // Close search when clicking outside
    $(document).on('click', function (e) {
        if (!$(e.target).closest('#searchContainer').length && 
            !$(e.target).closest('#searchToggle').length) {
            searchContainer.removeClass('show');
            searchInput.val('');
            searchResults.empty();
        }
    });

    // Helper to fetch and show users
    function fetchAndShowUsers(query) {
        $.ajax({
            url: '/Leaderboard/SearchUsers',
            method: 'GET',
            data: { query: query },
            success: function (data) {
                searchResults.empty();
                if (data.length === 0) {
                    searchResults.append('<div class="dropdown-item disabled">No users found</div>');
                } else {
                    searchResults.append('<h6 class="dropdown-header">Users</h6>');
                    data.forEach(function (user) {
                        let userItem = $('<a></a>')
                            .attr('href', '/Profile/UserProfile/' + user.id)
                            .addClass('dropdown-item')
                            .text(user.displayName);
                        if (user.followerCount > 0) {
                            userItem.append(' <small class="text-muted">(' + user.followerCount + ' followers)</small>');
                        }
                        searchResults.append(userItem);
                    });
                }
            },
            error: function () {
                searchResults.empty();
                searchResults.append('<div class="dropdown-item disabled">Error loading results</div>');
            }
        });
    }

    // Show top users when search input is focused
    searchInput.on('focus', function () {
        fetchAndShowUsers('');
    });

    // Handle search input with debounce
    searchInput.on('keyup', function () {
        const query = $(this).val().trim();
        
        // Clear previous timeout
        if (searchTimeout) {
            clearTimeout(searchTimeout);
        }
        
        // Empty results if query is empty
        if (query.length === 0) {
            searchResults.empty();
            return;
        }
        
        // Debounce the API call (300ms)
        searchTimeout = setTimeout(function() {
            fetchAndShowUsers(query);
        }, 300);
    });
});
