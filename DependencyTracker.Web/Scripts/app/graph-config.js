/* DependencyTracker graph styling configuration */
window.DTConfig = window.DTConfig || {};

(function (cfg) {
    'use strict';

    // Level-based node colors (visualizes the dependency chain depth)
    cfg.levelColors = [
        '#e11d48',   // L0 - root (rose)
        '#f59e0b',   // L1 (amber)
        '#84cc16',   // L2 (lime)
        '#14b8a6',   // L3 (teal)
        '#6366f1',   // L4 (indigo)
        '#8b5cf6',   // L5+ (violet)
        '#8b5cf6'
    ];

    cfg.levelColor = function (level) {
        level = Math.max(0, Math.min(level || 0, cfg.levelColors.length - 1));
        return cfg.levelColors[level];
    };

    // Criticality-based node accent (border + secondary ring)
    cfg.criticalityColor = function (criticality) {
        switch ((criticality || '').toLowerCase()) {
            case 'critical': return '#dc2626';
            case 'high': return '#f59e0b';
            case 'medium': return '#facc15';
            case 'low': return '#22c55e';
            default: return '#94a3b8';
        }
    };

    cfg.statusColor = function (status) {
        switch ((status || '').toLowerCase()) {
            case 'active': return '#14b8a6';
            case 'planned': return '#6366f1';
            case 'retired': return '#64748b';
            default: return '#94a3b8';
        }
    };

    // Node text color based on background luminance
    cfg.nodeTextColor = function (bg) {
        var c = bg.replace('#', '');
        var r = parseInt(c.substring(0, 2), 16);
        var g = parseInt(c.substring(2, 4), 16);
        var b = parseInt(c.substring(4, 6), 16);
        var lum = 0.299 * r + 0.587 * g + 0.114 * b;
        return lum > 150 ? '#1a1a1a' : '#ffffff';
    };

    cfg.typeColor = {
        'API': '#6366f1',
        'Database': '#14b8a6',
        'File': '#f97316',
        'Message': '#8b5cf6',
        'UI': '#f59e0b',
        'Infrastructure': '#64748b'
    };

    cfg.typeColorFor = function (type) {
        return cfg.typeColor[type] || '#94a3b8';
    };

    cfg.edgeLabel = function (edge) {
        var label = edge.type || '';
        if (edge.criticality === 'Critical') label = '\u26a0 ' + label;
        return label;
    };

    cfg.legendLevelLabel = function (level) {
        return 'L' + level;
    };
})(window.DTConfig);
