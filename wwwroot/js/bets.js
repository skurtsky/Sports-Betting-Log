document.addEventListener('DOMContentLoaded', function() {    // Handle clear filters button
    document.getElementById('clearFilters')?.addEventListener('click', function() {
        // Reset all form inputs
        const form = document.getElementById('filterForm');
        const selects = form.querySelectorAll('select');
        const inputs = form.querySelectorAll('input:not([type="hidden"])');
        
        selects.forEach(select => {
            select.value = '';
        });
        
        inputs.forEach(input => {
            input.value = '';
        });
        
        // Add resetFilters parameter
        const resetInput = document.createElement('input');
        resetInput.type = 'hidden';
        resetInput.name = 'resetFilters';
        resetInput.value = 'true';
        form.appendChild(resetInput);
        
        // Submit the form to refresh the page
        form.submit();
    });

    // Handle sorting while maintaining filters
    document.querySelectorAll('th a[href*="sortOrder"]').forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            
            // Get current filter values
            const form = document.getElementById('filterForm');
            document.querySelector('input[name="sortOrder"]').value = this.getAttribute('href').split('=')[1];
            
            // Submit form with current filters and new sort order
            form.submit();
        });
    });

    // Add event listener for bulk selection
    const selectAllCheckbox = document.querySelector('input[type="checkbox"][data-select-all]');
    if (selectAllCheckbox) {
        selectAllCheckbox.addEventListener('change', function() {
            const isChecked = this.checked;
            document.querySelectorAll('input[type="checkbox"][data-bet-id]').forEach(checkbox => {
                checkbox.checked = isChecked;
            });
            updateBulkActionsVisibility();
        });
    }

    // Add event listeners for individual checkboxes
    document.querySelectorAll('input[type="checkbox"][data-bet-id]').forEach(checkbox => {
        checkbox.addEventListener('change', updateBulkActionsVisibility);
    });

    // Function to update bulk actions visibility
    function updateBulkActionsVisibility() {
        const checkedBoxes = document.querySelectorAll('input[type="checkbox"][data-bet-id]:checked');
        const bulkActions = document.querySelector('.bulk-actions');
        if (checkedBoxes.length > 0) {
            bulkActions.classList.remove('d-none');
        } else {
            bulkActions.classList.add('d-none');
        }
    }
});
