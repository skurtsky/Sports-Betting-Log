/**
 * Kelly Calculator for Sports Betting
 */
document.addEventListener("DOMContentLoaded", function() {
    // Kelly Calculator functionality
    const kellyForm = document.getElementById('kellyCalculatorForm');
    if (kellyForm) {
        kellyForm.addEventListener('submit', function(e) {
            // Client-side form validation is handled by the browser
            // Additional validation can be added here if needed
        });
        
        // Add real-time preview for Kelly calculation
        const bankrollInput = document.getElementById('CurrentBankroll');
        const winPercentInput = document.getElementById('EstimatedWinPercentage');
        const oddsInput = document.getElementById('DecimalOdds');
        const previewElement = document.getElementById('kellyPreview');
        
        if (bankrollInput && winPercentInput && oddsInput && previewElement) {
            const updatePreview = function() {
                const bankroll = parseFloat(bankrollInput.value) || 0;
                const winPercent = parseFloat(winPercentInput.value) || 0;
                const odds = parseFloat(oddsInput.value) || 0;
                
                if (bankroll > 0 && winPercent > 0 && winPercent < 100 && odds > 1) {
                    // Convert win percentage to probability (0-1)
                    const winProb = winPercent / 100;
                    const lossProb = 1 - winProb;
                    const b = odds - 1; // Net odds (decimal odds - 1)
                    
                    // Kelly formula: f = (bp - q) / b
                    // where f is fraction of bankroll, b is net odds, p is win probability, q is loss probability
                    let kellyFraction = (b * winProb - lossProb) / b;
                    
                    // Cap the Kelly bet at 25% of bankroll as a safety measure
                    kellyFraction = Math.min(kellyFraction, 0.25);
                    
                    // Ensure we don't recommend negative bets
                    kellyFraction = Math.max(0, kellyFraction);
                    
                    const kellyAmount = bankroll * kellyFraction;
                    const halfKelly = kellyAmount / 2;
                    const quarterKelly = kellyAmount / 4;
                    
                    // Update the preview with calculated values
                    previewElement.innerHTML = `
                        <div class="alert alert-info">
                            <h6>Estimated Results:</h6>
                            <p class="mb-1"><strong>Full Kelly:</strong> $${kellyAmount.toFixed(2)} (${(kellyFraction * 100).toFixed(1)}%)</p>
                            <p class="mb-1"><strong>Half Kelly:</strong> $${halfKelly.toFixed(2)} (${(kellyFraction * 50).toFixed(1)}%)</p>
                            <p class="mb-0"><strong>Quarter Kelly:</strong> $${quarterKelly.toFixed(2)} (${(kellyFraction * 25).toFixed(1)}%)</p>
                        </div>
                        <div class="alert alert-warning">
                            <small>This is just a preview. Submit the form for full calculation.</small>
                        </div>
                    `;
                    previewElement.style.display = 'block';
                } else {
                    previewElement.style.display = 'none';
                }
            };
            
            // Add event listeners to update preview on input change
            bankrollInput.addEventListener('input', updatePreview);
            winPercentInput.addEventListener('input', updatePreview);
            oddsInput.addEventListener('input', updatePreview);
            
            // Also add listener for American odds converter if it exists
            const americanOddsInput = document.getElementById('AmericanOdds');
            if (americanOddsInput) {
                americanOddsInput.addEventListener('input', function() {
                    const americanOdds = parseInt(americanOddsInput.value);
                    if (!isNaN(americanOdds)) {
                        // Convert American odds to decimal
                        let decimalOdds;
                        if (americanOdds > 0) {
                            decimalOdds = (americanOdds / 100) + 1;
                        } else if (americanOdds < 0) {
                            decimalOdds = (100 / Math.abs(americanOdds)) + 1;
                        } else {
                            decimalOdds = 1;
                        }
                        
                        oddsInput.value = decimalOdds.toFixed(2);
                        updatePreview();
                    }
                });
            }
        }
    }
});
