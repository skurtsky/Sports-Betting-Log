/**
 * Calendar functionality for Sports Betting Tracker
 */
document.addEventListener('DOMContentLoaded', function() {
    // Handle "more bets" click to navigate to daily details
    document.querySelectorAll('.more-bets').forEach(function(element) {
        element.addEventListener('click', function(e) {
            const dayCell = e.target.closest('.calendar-day');
            if (dayCell) {
                const dayLink = dayCell.querySelector('.day-number');
                if (dayLink) {
                    dayLink.click();
                }
            }
        });
    });

    // Add hover effect to days with bets
    document.querySelectorAll('.calendar-day').forEach(function(day) {
        if (day.querySelector('.bet-item')) {
            day.classList.add('has-bets');
            
            day.addEventListener('click', function(e) {
                if (!e.target.closest('a') && !e.target.closest('button')) {
                    const dayLink = day.querySelector('.day-number');
                    if (dayLink) {
                        dayLink.click();
                    }
                }
            });
        }
    });

    // Today button functionality
    const todayButton = document.getElementById('goToToday');
    if (todayButton) {
        todayButton.addEventListener('click', function() {
            const today = new Date();
            window.location.href = `/Calendar/Index?year=${today.getFullYear()}&month=${today.getMonth() + 1}`;
        });
    }

    // Initialize tooltips
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
        const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));
    }

    // Refresh monthly stats periodically
    const refreshStats = async () => {
        try {
            const urlParams = new URLSearchParams(window.location.search);
            const year = urlParams.get('year') || new Date().getFullYear();
            const month = urlParams.get('month') || (new Date().getMonth() + 1);

            const response = await fetch(`/api/Calendar/MonthSummary?year=${year}&month=${month}`);
            if (!response.ok) throw new Error('Failed to fetch stats');
            
            const data = await response.json();
            
            // Update stats
            document.getElementById('totalBets').textContent = data.totalBets;
            document.getElementById('monthlyProfit').textContent = data.formattedProfit;
            document.getElementById('winLossDays').textContent = `${data.winningDays} W - ${data.losingDays} L`;
            
            // Update profit card color
            const profitCard = document.querySelector('.profit-card');
            if (profitCard) {
                profitCard.classList.remove('bg-success', 'bg-danger');
                profitCard.classList.add(data.totalProfit >= 0 ? 'bg-success' : 'bg-danger');
            }
        } catch (error) {
            console.error('Error refreshing stats:', error);
        }
    };

    // Set up periodic refresh (every 5 minutes)
    setInterval(refreshStats, 300000);
});
