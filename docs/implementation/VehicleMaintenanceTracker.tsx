import React, { useEffect, useRef, useState } from 'react';
import * as echarts from 'echarts';
import axios from 'axios';

interface VehicleMaintenanceStatus {
  vehicleId: number;
  licensePlate: string;
  modelName: string;
  estimatedCurrentKm: number;
  lastMaintenanceKm: number;
  lastMaintenanceDate: string | null;
  nextMaintenanceKm: number;
  averageKmPerDay: number;
  remainingKm: number;
  estimatedDaysUntilMaintenance: number;
  estimatedNextMaintenanceDate: string | null;
  progressPercent: number;
  status: 'Normal' | 'NeedAttention' | 'Urgent';
  message: string;
  hasSufficientHistory: boolean;
  historyCount: number;
}

interface VehicleMaintenanceTrackerProps {
  vehicleId: number;
  apiBaseUrl: string;
  authToken: string;
}

export default function VehicleMaintenanceTracker({
  vehicleId,
  apiBaseUrl,
  authToken
}: VehicleMaintenanceTrackerProps) {
  const chartRef = useRef<HTMLDivElement>(null);
  const [maintenanceStatus, setMaintenanceStatus] = useState<VehicleMaintenanceStatus | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Fetch maintenance status from API
  useEffect(() => {
    const fetchMaintenanceStatus = async () => {
      try {
        setLoading(true);
        const response = await axios.get(
          `${apiBaseUrl}/api/VehicleMaintenance/${vehicleId}/status`,
          {
            headers: {
              Authorization: `Bearer ${authToken}`
            }
          }
        );

        if (response.data.success) {
          setMaintenanceStatus(response.data.data);
        } else {
          setError('Không thể tải trạng thái bảo dưỡng');
        }
      } catch (err) {
        setError('Lỗi khi kết nối API');
        console.error('Error fetching maintenance status:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchMaintenanceStatus();
  }, [vehicleId, apiBaseUrl, authToken]);

  // Render gauge chart with ECharts
  useEffect(() => {
    if (!chartRef.current || !maintenanceStatus) return;

    const chart = echarts.init(chartRef.current);

    // Determine color based on status
    const getStatusColor = (status: string) => {
      switch (status) {
        case 'Urgent':
          return '#FF4444'; // Red
        case 'NeedAttention':
          return '#FFA500'; // Orange
        default:
          return '#4CAF50'; // Green
      }
    };

    const option: echarts.EChartsOption = {
      series: [
        {
          type: 'gauge',
          startAngle: 180,
          endAngle: 0,
          min: 0,
          max: 100,
          center: ['50%', '75%'],
          radius: '90%',
          splitNumber: 10,
          axisLine: {
            lineStyle: {
              width: 30,
              color: [
                [0.7, '#4CAF50'],     // 0-70%: Green (Normal)
                [0.9, '#FFA500'],     // 70-90%: Orange (NeedAttention)
                [1, '#FF4444']        // 90-100%: Red (Urgent)
              ]
            }
          },
          pointer: {
            itemStyle: {
              color: 'auto'
            },
            length: '70%',
            width: 8
          },
          axisTick: {
            distance: -30,
            length: 8,
            lineStyle: {
              color: '#fff',
              width: 2
            }
          },
          splitLine: {
            distance: -30,
            length: 20,
            lineStyle: {
              color: '#fff',
              width: 4
            }
          },
          axisLabel: {
            color: 'inherit',
            distance: 40,
            fontSize: 14,
            formatter: (value: number) => `${value}%`
          },
          detail: {
            valueAnimation: true,
            formatter: '{value}%',
            color: 'inherit',
            fontSize: 36,
            offsetCenter: [0, '-15%']
          },
          data: [
            {
              value: Math.round(maintenanceStatus.progressPercent),
              name: maintenanceStatus.status === 'Urgent'
                ? 'CẬN BẢO DƯỠNG'
                : maintenanceStatus.status === 'NeedAttention'
                  ? 'CẦN CHÚ Ý'
                  : 'BÌNH THƯỜNG'
            }
          ],
          title: {
            offsetCenter: [0, '30%'],
            fontSize: 18,
            color: getStatusColor(maintenanceStatus.status)
          }
        }
      ]
    };

    chart.setOption(option);

    // Cleanup
    return () => {
      chart.dispose();
    };
  }, [maintenanceStatus]);

  if (loading) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="text-gray-600">Đang tải...</div>
      </div>
    );
  }

  if (error || !maintenanceStatus) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="text-red-600">{error || 'Không có dữ liệu'}</div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-lg p-6">
      {/* Header */}
      <div className="mb-6">
        <h3 className="text-2xl font-bold text-gray-800">
          Trạng thái bảo dưỡng
        </h3>
        <p className="text-gray-600">
          {maintenanceStatus.licensePlate} - {maintenanceStatus.modelName}
        </p>
      </div>

      {/* Gauge Chart */}
      <div
        ref={chartRef}
        className="w-full"
        style={{ height: '300px' }}
      />

      {/* Status Message */}
      <div className={`mt-4 p-4 rounded-lg ${
        maintenanceStatus.status === 'Urgent'
          ? 'bg-red-50 border border-red-200'
          : maintenanceStatus.status === 'NeedAttention'
            ? 'bg-orange-50 border border-orange-200'
            : 'bg-green-50 border border-green-200'
      }`}>
        <p className={`text-sm font-medium ${
          maintenanceStatus.status === 'Urgent'
            ? 'text-red-800'
            : maintenanceStatus.status === 'NeedAttention'
              ? 'text-orange-800'
              : 'text-green-800'
        }`}>
          {maintenanceStatus.message}
        </p>
      </div>

      {/* Detailed Statistics */}
      <div className="mt-6 grid grid-cols-2 gap-4">
        <div className="p-4 bg-gray-50 rounded-lg">
          <p className="text-sm text-gray-600">Km ước tính hiện tại</p>
          <p className="text-2xl font-bold text-gray-800">
            {maintenanceStatus.estimatedCurrentKm.toLocaleString()} km
          </p>
        </div>

        <div className="p-4 bg-gray-50 rounded-lg">
          <p className="text-sm text-gray-600">Km còn lại</p>
          <p className="text-2xl font-bold text-gray-800">
            {maintenanceStatus.remainingKm.toLocaleString()} km
          </p>
        </div>

        <div className="p-4 bg-gray-50 rounded-lg">
          <p className="text-sm text-gray-600">Trung bình km/ngày</p>
          <p className="text-2xl font-bold text-gray-800">
            {maintenanceStatus.averageKmPerDay.toFixed(1)} km
          </p>
        </div>

        <div className="p-4 bg-gray-50 rounded-lg">
          <p className="text-sm text-gray-600">Ngày còn lại</p>
          <p className="text-2xl font-bold text-gray-800">
            {maintenanceStatus.estimatedDaysUntilMaintenance} ngày
          </p>
        </div>

        {maintenanceStatus.lastMaintenanceDate && (
          <div className="p-4 bg-gray-50 rounded-lg col-span-2">
            <p className="text-sm text-gray-600">Lần bảo dưỡng gần nhất</p>
            <p className="text-lg font-semibold text-gray-800">
              {new Date(maintenanceStatus.lastMaintenanceDate).toLocaleDateString('vi-VN')}
              {' - '}
              {maintenanceStatus.lastMaintenanceKm.toLocaleString()} km
            </p>
          </div>
        )}

        {maintenanceStatus.estimatedNextMaintenanceDate && (
          <div className="p-4 bg-blue-50 border border-blue-200 rounded-lg col-span-2">
            <p className="text-sm text-blue-600">Dự kiến bảo dưỡng tiếp theo</p>
            <p className="text-lg font-semibold text-blue-800">
              {new Date(maintenanceStatus.estimatedNextMaintenanceDate).toLocaleDateString('vi-VN')}
              {' - '}
              {maintenanceStatus.nextMaintenanceKm.toLocaleString()} km
            </p>
          </div>
        )}
      </div>

      {/* Data Quality Notice */}
      {!maintenanceStatus.hasSufficientHistory && (
        <div className="mt-4 p-3 bg-yellow-50 border border-yellow-200 rounded-lg">
          <p className="text-sm text-yellow-800">
            Chưa đủ dữ liệu lịch sử để ước tính chính xác.
            Hiện có {maintenanceStatus.historyCount} lần bảo dưỡng (cần tối thiểu 2 lần).
          </p>
        </div>
      )}
    </div>
  );
}

// Example usage component
export function MyVehiclesMaintenanceTracker({
  apiBaseUrl,
  authToken
}: {
  apiBaseUrl: string;
  authToken: string;
}) {
  const [vehicles, setVehicles] = useState<VehicleMaintenanceStatus[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchAllVehicles = async () => {
      try {
        const response = await axios.get(
          `${apiBaseUrl}/api/VehicleMaintenance/my-vehicles/status`,
          {
            headers: {
              Authorization: `Bearer ${authToken}`
            }
          }
        );

        if (response.data.success) {
          setVehicles(response.data.data);
        }
      } catch (err) {
        console.error('Error fetching vehicles:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchAllVehicles();
  }, [apiBaseUrl, authToken]);

  if (loading) {
    return <div className="p-8 text-center">Đang tải...</div>;
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 p-6">
      {vehicles.map((vehicle) => (
        <VehicleMaintenanceTracker
          key={vehicle.vehicleId}
          vehicleId={vehicle.vehicleId}
          apiBaseUrl={apiBaseUrl}
          authToken={authToken}
        />
      ))}
    </div>
  );
}
