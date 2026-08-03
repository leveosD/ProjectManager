import { useState, useEffect, useRef } from 'react';
import { employeeService } from '@infrastructure';
import './EmployeeSearch.css';

const EmployeeSearch = ({
  selected,
  onSelect,
  placeholder = 'Search employee…',
  multiple = false,
  onMultiSelect,
  employees: employeesProp,
  roleFilter = [],
  excludeEmployees = [],
  inputId,
}) => {
  const [query, setQuery] = useState('');
  const [employees, setEmployees] = useState([]);
  const [loading, setLoading] = useState(false);
  const [showDropdown, setShowDropdown] = useState(false);
  const [selectedEmployees, setSelectedEmployees] = useState(multiple ? (selected || []) : []);
  const inputRef = useRef(null);
  const wrapperRef = useRef(null);

  useEffect(() => {
    const timer = setTimeout(() => {
      if (!query.trim()) return;
      // Don't re-search if the query already matches the selected employee
      if (!multiple && selected?.fullName && query.trim() === selected.fullName) return;
      searchEmployees(query);
    }, 300);

    return () => clearTimeout(timer);
  }, [query, selected, multiple]);

  useEffect(() => {
    if (multiple) {
      setSelectedEmployees(selected || []);
    } else {
      setQuery(selected?.fullName ?? '');
    }
  }, [selected, multiple]);

  const searchEmployees = async (searchQuery) => {
    setLoading(true);
    try {
      let results;
      if (employeesProp) {
        const term = searchQuery.trim().toLowerCase();
        results = term
          ? employeesProp.filter((emp) =>
              emp.fullName?.toLowerCase().includes(term) ||
              emp.email?.toLowerCase().includes(term)
            )
          : employeesProp;
      } else {
        results = searchQuery.trim()
          ? await employeeService.search(searchQuery, roleFilter)
          : await employeeService.getAll(roleFilter);
      }

      if (roleFilter.length > 0) {
        results = results.filter((emp) => roleFilter.includes(emp.role));
      }

      // Filter out already selected employees and explicitly excluded employees
      const excludedIds = new Set([
        ...selectedEmployees.map((sel) => sel.id),
        ...excludeEmployees.map((ex) => ex.id),
      ]);
      const filtered = results.filter((emp) => !excludedIds.has(emp.id));
      setEmployees(filtered);
      setShowDropdown(true);
    } catch {
      setEmployees([]);
    } finally {
      setLoading(false);
    }
  };

  const handleSelect = (employee) => {
    if (multiple) {
      const updated = [...selectedEmployees, employee];
      setSelectedEmployees(updated);
      setQuery('');
      setEmployees([]);
      setShowDropdown(false);
      onMultiSelect?.(updated);
    } else {
      onSelect?.(employee);
      setQuery(employee?.fullName ?? '');
      setShowDropdown(false);
    }
  };

  const removeSelected = (employeeId) => {
    const updated = selectedEmployees.filter((e) => e.id !== employeeId);
    setSelectedEmployees(updated);
    onMultiSelect?.(updated);
    inputRef.current?.focus();
  };

  useEffect(() => {
    const handleClickOutside = (event) => {
      if (wrapperRef.current && !wrapperRef.current.contains(event.target)) {
        setShowDropdown(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  return (
    <div className="employee-search" ref={wrapperRef}>
      {multiple && selectedEmployees.length > 0 && (
        <div className="selected-chips">
          {selectedEmployees.map((emp) => (
            <span key={emp.id} className="chip">
              {emp.fullName}
              <button
                type="button"
                onClick={() => removeSelected(emp.id)}
                className="chip-remove"
                aria-label={`Remove ${emp.fullName}`}
              >
                ×
              </button>
            </span>
          ))}
        </div>
      )}
      <input
        ref={inputRef}
        id={inputId}
        type="text"
        className="form-control"
        placeholder={placeholder}
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        onFocus={() => {
          if (!query.trim()) {
            searchEmployees('');
          } else if (employees.length > 0) {
            setShowDropdown(true);
          }
        }}
        role="combobox"
        aria-autocomplete="list"
        aria-expanded={showDropdown}
        aria-controls={inputId ? `${inputId}-listbox` : undefined}
        aria-activedescendant={undefined}
        autoComplete="off"
      />
      {showDropdown && (
        <div
          id={inputId ? `${inputId}-listbox` : undefined}
          className="dropdown"
          role="listbox"
        >
          {loading ? (
            <div className="dropdown-item">Loading…</div>
          ) : employees.length > 0 ? (
            employees.map((emp) => (
              <div
                key={emp.id}
                className="dropdown-item"
                role="option"
                tabIndex={0}
                onClick={() => handleSelect(emp)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    handleSelect(emp);
                  }
                }}
              >
                <strong>{emp.fullName}</strong>
                <span className="employee-email">{emp.email}</span>
              </div>
            ))
          ) : (
            <div className="dropdown-item">No employees found</div>
          )}
        </div>
      )}
    </div>
  );
};

export default EmployeeSearch;
